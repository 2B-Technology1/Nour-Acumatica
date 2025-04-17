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
using PX.Objects.CR;
using PX.Objects.CR.Workflows;

namespace PX.Objects.CR.Standalone
{
	/// <inheritdoc cref="CR.Location" />
	[Serializable()]
	[PXPrimaryGraph(
		new Type[] {
			typeof(VendorLocationMaint),
			typeof(CustomerLocationMaint),
			typeof(BranchMaint),
			typeof(EP.EmployeeMaint),
			typeof(AccountLocationMaint)},

		new Type[] {
			typeof(Select<CR.Location,
				Where<
					CR.Location.bAccountID, Equal<Current<Location.bAccountID>>,
					And<CR.Location.locationID, Equal<Current<Location.locationID>>,
					And<Where<Current<Location.locType>, Equal<LocTypeList.vendorLoc>,
						Or<Current<Location.locType>, Equal<LocTypeList.combinedLoc>>>>>>>),
			typeof(Select<CR.Location,
				Where<
					CR.Location.bAccountID, Equal<Current<Location.bAccountID>>,
					And<CR.Location.locationID, Equal<Current<Location.locationID>>,
					And<Where<Current<Location.locType>, Equal<LocTypeList.customerLoc>,
						Or<Current<Location.locType>, Equal<LocTypeList.combinedLoc>>>>>>>),
			typeof(Select2<Branch,
				InnerJoin<BAccount,
					On<BAccount.bAccountID, Equal<Branch.bAccountID>>,
				InnerJoin<CR.Location,
					On<CR.Location.bAccountID, Equal<BAccount.bAccountID>,
					And<CR.Location.locationID, Equal<BAccount.defLocationID>>>>>,
				Where<
					CR.Location.bAccountID, Equal<Current<Location.bAccountID>>,
					And<CR.Location.locationID, Equal<Current<Location.locationID>>,
					And<Current<Location.locType>, Equal<LocTypeList.companyLoc>>>>>),
			typeof(Select2<EP.EPEmployee,
				InnerJoin<CR.Location,
					On<CR.Location.bAccountID, Equal<EP.EPEmployee.bAccountID>,
					And<CR.Location.locationID, Equal<EP.EPEmployee.defLocationID>>>>,
				Where<
					CR.Location.bAccountID, Equal<Current<Location.bAccountID>>,
					And<CR.Location.locationID, Equal<Current<Location.locationID>>,
					And<Current<Location.locType>, Equal<LocTypeList.employeeLoc>>>>>),
			typeof(Select<Location,
				Where<
					CR.Location.bAccountID, Equal<Current<Location.bAccountID>>,
					And<CR.Location.locationID, Equal<Current<Location.locationID>>,
					And<Where<Current<Location.locType>, Equal<LocTypeList.companyLoc>,
						Or<Current<Location.locType>, Equal<LocTypeList.combinedLoc>>>>>>>)
		})]
	public partial class Location : PX.Data.IBqlTable, ILocation
	{
		#region Keys

		/// <summary>
		/// Primary Key
		/// </summary>
		public new class PK : PrimaryKeyOf<Location>.By<locationID>
		{
			public static Location Find(PXGraph graph, int? locationID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, locationID, options);
		}
		#endregion

		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }

		/// <inheritdoc cref="CR.Location.LocationID"/>
		[PXDBIdentity()]
		[PXUIField(Visible = false, Enabled = false, Visibility = PXUIVisibility.Invisible)]
		[PXReferentialIntegrityCheck]
		public virtual Int32? LocationID { get; set; }
		#endregion
		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }

		/// <inheritdoc cref="CR.Location.BAccountID"/>
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Account ID", Visible = false, Enabled = false, Visibility = PXUIVisibility.Invisible, TabOrder = 0)]
		public virtual Int32? BAccountID { get; set; }
		#endregion
		#region LocationCD
		public abstract class locationCD : PX.Data.BQL.BqlString.Field<locationCD> { }

		/// <inheritdoc cref="CR.Location.LocationCD"/>
		[PXDBString(IsKey = true, IsUnicode = true)]
		[PXDimension(LocationAttribute.DimensionName)]
		public virtual String LocationCD { get; set; }
		#endregion

		#region LocType
		public abstract class locType : PX.Data.BQL.BqlString.Field<locType> { }

		/// <inheritdoc cref="CR.Location.LocType"/>
		[PXDBString(2, IsFixed = true)]
		[LocTypeList.List()]
		[PXUIField(DisplayName = "Location Type")]
		[PXDefault]
		public virtual String LocType { get; set; }
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

		/// <inheritdoc cref="CR.Location.Descr"/>
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Location Name")]
		public virtual String Descr { get; set; }
		#endregion
		#region TaxRegistrationID
		public abstract class taxRegistrationID : PX.Data.BQL.BqlString.Field<taxRegistrationID> { }

		/// <inheritdoc cref="CR.Location.TaxRegistrationID"/>
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Registration ID")]
		[PXPersonalDataField]
		[PXDeduplicationSearchField]
		public virtual String TaxRegistrationID { get; set; }
		#endregion
		#region DefAddressID
		public abstract class defAddressID : PX.Data.BQL.BqlInt.Field<defAddressID> { }

		/// <inheritdoc cref="CR.Location.DefAddressID"/>
		[PXDBInt()]
		[PXForeignReference(typeof(Field<Location.defAddressID>.IsRelatedTo<Address.addressID>))]
		[PXUIField(DisplayName = "Default Address")]
		public virtual Int32? DefAddressID { get; set; }
		#endregion
		#region Override Address
		public abstract class overrideAddress : PX.Data.BQL.BqlBool.Field<overrideAddress> { }

		/// <inheritdoc cref="CR.Location.OverrideAddress"/>
		[PXBool]
		[PXUIField(DisplayName = "Override")]
		public virtual bool? OverrideAddress { get; set; }
		#endregion
		#region IsAddressSameAsMain
		[Obsolete("Use OverrideAddress instead")]
		public abstract class isAddressSameAsMain : PX.Data.BQL.BqlBool.Field<isAddressSameAsMain> { }

		/// <inheritdoc cref="CR.Location.IsAddressSameAsMain"/>
		[Obsolete("Use OverrideAddress instead")]
		[PXBool()]
		[PXUIField(DisplayName = "Same as Main")]
		public virtual bool? IsAddressSameAsMain
		{
			get { return OverrideAddress != null ? !this.OverrideAddress : null; }
		}
		#endregion
		#region DefContactID
		public abstract class defContactID : PX.Data.BQL.BqlInt.Field<defContactID> { }

		/// <inheritdoc cref="CR.Location.DefContactID"/>
		[PXDBInt()]
		[PXForeignReference(typeof(Field<Location.defContactID>.IsRelatedTo<Contact.contactID>))]
		[PXUIField(DisplayName = "Default Contact")]
		public virtual Int32? DefContactID { get; set; }
		#endregion
		#region Override Contact
		public abstract class overrideContact : PX.Data.BQL.BqlBool.Field<overrideContact> { }

		/// <inheritdoc cref="CR.Location.OverrideContact"/>
		[PXBool]
		[PXUIField(DisplayName = "Override")]
		public virtual bool? OverrideContact { get; set; }
		#endregion
		#region IsContactSameAsMain
		[Obsolete("Use OverrideContact instead")]
		public abstract class isContactSameAsMain : PX.Data.BQL.BqlBool.Field<isContactSameAsMain> { }

		/// <inheritdoc cref="CR.Location.IsContactSameAsMain"/>
		[Obsolete("Use OverrideContact instead")]
		[PXBool()]
		[PXUIField(DisplayName = "Same as Main")]
		public virtual bool? IsContactSameAsMain
		{
			get { return OverrideContact != null ? !this.OverrideContact : null; }
		}
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXNote()]
		public virtual Guid? NoteID { get; set; }
		#endregion
		#region IsActive
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }

		/// <inheritdoc cref="CR.Location.IsActive"/>
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual bool? IsActive { get; set; }
		#endregion
		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }

		/// <inheritdoc cref="CR.Location.Status"/>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(LocationStatus.Active)]
		[PXUIField(DisplayName = "Status")]
		[LocationStatus.List]
		public virtual string Status { get; set; }
		#endregion
		#region IsDefault
		public abstract class isDefault : PX.Data.BQL.BqlBool.Field<isDefault> { }

		/// <inheritdoc cref="CR.Location.IsDefault"/>
		[PXBool]
		[PXUIField(DisplayName = "Default", Enabled = false)]
		public virtual bool? IsDefault { get; set; }
		#endregion



		//Customer Location Properties
		#region CTaxZoneID
		public abstract class cTaxZoneID : PX.Data.BQL.BqlString.Field<cTaxZoneID> { }

		/// <inheritdoc cref="CR.Location.CTaxZoneID"/>
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Zone")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String CTaxZoneID { get; set; }
		#endregion
		#region CTaxCalcMode
		public abstract class cTaxCalcMode : PX.Data.BQL.BqlString.Field<cTaxCalcMode> { }

		/// <inheritdoc cref="CR.Location.CTaxCalcMode"/>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(TaxCalculationMode.TaxSetting, typeof(Search<CustomerClass.taxCalcMode, Where<CustomerClass.customerClassID, Equal<Current<CustomerClass.customerClassID>>>>))]
		[TaxCalculationMode.List]
		[PXUIField(DisplayName = "Tax Calculation Mode")]
		public virtual string CTaxCalcMode { get; set; }
		#endregion
		#region CAvalaraExemptionNumber
		public abstract class cAvalaraExemptionNumber : PX.Data.BQL.BqlString.Field<cAvalaraExemptionNumber> { }

		/// <inheritdoc cref="CR.Location.CAvalaraExemptionNumber"/>
		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Exemption Number")]
		public virtual String CAvalaraExemptionNumber { get; set; }
		#endregion
		#region CAvalaraCustomerUsageType
		public abstract class cAvalaraCustomerUsageType : PX.Data.BQL.BqlString.Field<cAvalaraCustomerUsageType> { }

		/// <inheritdoc cref="CR.Location.CAvalaraCustomerUsageType"/>
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Entity Usage Type")]
		[PXDefault(TXAvalaraCustomerUsageType.Default)]
		[TX.TXAvalaraCustomerUsageType.List]
		public virtual String CAvalaraCustomerUsageType { get; set; }
		#endregion
		#region CCarrierID
		public abstract class cCarrierID : PX.Data.BQL.BqlString.Field<cCarrierID> { }

		/// <inheritdoc cref="CR.Location.CCarrierID"/>
		[PXDBString(15, IsUnicode = true, InputMask = ">aaaaaaaaaaaaaaa")]
		[PXUIField(DisplayName = "Ship Via")]
		public virtual String CCarrierID { get; set; }
		#endregion
		#region CShipTermsID
		public abstract class cShipTermsID : PX.Data.BQL.BqlString.Field<cShipTermsID> { }

		/// <inheritdoc cref="CR.Location.CShipTermsID"/>
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Shipping Terms")]
		public virtual String CShipTermsID { get; set; }
		#endregion
		#region CShipZoneID
		public abstract class cShipZoneID : PX.Data.BQL.BqlString.Field<cShipZoneID> { }

		/// <inheritdoc cref="CR.Location.CShipZoneID"/>
		[PXDBString(15, IsUnicode = true, InputMask = ">aaaaaaaaaaaaaaa")]
		[PXUIField(DisplayName = "Shipping Zone")]
		public virtual String CShipZoneID { get; set; }
		#endregion
		#region CFOBPointID
		public abstract class cFOBPointID : PX.Data.BQL.BqlString.Field<cFOBPointID> { }

		/// <inheritdoc cref="CR.Location.CFOBPointID"/>
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "FOB Point")]
		public virtual String CFOBPointID { get; set; }
		#endregion
		#region CResedential
		public abstract class cResedential : PX.Data.BQL.BqlBool.Field<cResedential> { }

		/// <inheritdoc cref="CR.Location.CResedential"/>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Residential Delivery")]
		public virtual Boolean? CResedential { get; set; }
		#endregion
		#region CSaturdayDelivery
		public abstract class cSaturdayDelivery : PX.Data.BQL.BqlBool.Field<cSaturdayDelivery> { }

		/// <inheritdoc cref="CR.Location.CSaturdayDelivery"/>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Saturday Delivery")]
		public virtual Boolean? CSaturdayDelivery { get; set; }
		#endregion
		#region CGroundCollect
		public abstract class cGroundCollect : PX.Data.BQL.BqlBool.Field<cGroundCollect> { }

		/// <inheritdoc cref="CR.Location.CGroundCollect"/>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Ground Collect")]
		public virtual Boolean? CGroundCollect { get; set; }
		#endregion
		#region CInsurance
		public abstract class cInsurance : PX.Data.BQL.BqlBool.Field<cInsurance> { }

		/// <inheritdoc cref="CR.Location.CInsurance"/>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Insurance")]
		public virtual Boolean? CInsurance { get; set; }
		#endregion
		#region CLeadTime
		public abstract class cLeadTime : PX.Data.BQL.BqlShort.Field<cLeadTime> { }

		/// <inheritdoc cref="CR.Location.CLeadTime"/>
		[PXDBShort(MinValue = 0, MaxValue = 100000)]
		[PXUIField(DisplayName = CR.Messages.LeadTimeDays)]
		public virtual Int16? CLeadTime { get; set; }
		#endregion
		#region CBranchID
		public abstract class cBranchID : PX.Data.BQL.BqlInt.Field<cBranchID> { }

		/// <inheritdoc cref="CR.Location.CBranchID"/>
		[PXDBInt()]
		[PXUIField(DisplayName = "Shipping Branch")]
		public virtual Int32? CBranchID { get; set; }
		#endregion
		#region CSalesAcctID
		public abstract class cSalesAcctID : PX.Data.BQL.BqlInt.Field<cSalesAcctID> { }

		/// <inheritdoc cref="CR.Location.CSalesAcctID"/>
		[PXDBInt()]
		[PXUIField(DisplayName = "Sales Account")]
		public virtual Int32? CSalesAcctID { get; set; }
		#endregion
		#region CSalesSubID
		public abstract class cSalesSubID : PX.Data.BQL.BqlInt.Field<cSalesSubID> { }

		/// <inheritdoc cref="CR.Location.CSalesSubID"/>
		[PXDBInt()]
		[PXUIField(DisplayName = "Sales Sub.")]
		public virtual Int32? CSalesSubID { get; set; }
		#endregion
		#region CPriceClassID
		public abstract class cPriceClassID : PX.Data.BQL.BqlString.Field<cPriceClassID> { }

		/// <inheritdoc cref="CR.Location.CPriceClassID"/>
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Price Class")]
		public virtual String CPriceClassID { get; set; }
		#endregion
		#region CSiteID
		public abstract class cSiteID : PX.Data.BQL.BqlInt.Field<cSiteID> { }

		/// <inheritdoc cref="CR.Location.CSiteID"/>
		[PXDBInt()]
		[PXUIField(DisplayName = "Warehouse")]
		[PXForeignReference(typeof(Field<cSiteID>.IsRelatedTo<INSite.siteID>))]
		public virtual Int32? CSiteID { get; set; }
		#endregion
		#region CDiscountAcctID
		public abstract class cDiscountAcctID : PX.Data.BQL.BqlInt.Field<cDiscountAcctID> { }

		/// <inheritdoc cref="CR.Location.CDiscountAcctID"/>
		[PXDBInt()]
		[PXUIField(DisplayName = "Discount Account")]
		public virtual Int32? CDiscountAcctID { get; set; }
		#endregion
		#region CDiscountSubID
		public abstract class cDiscountSubID : PX.Data.BQL.BqlInt.Field<cDiscountSubID> { }

		/// <inheritdoc cref="CR.Location.CDiscountSubID"/>
		[PXDBInt()]
		[PXUIField(DisplayName = "Discount Sub.")]
		public virtual Int32? CDiscountSubID { get; set; }
		#endregion
		#region CRetainageAcctID
		public abstract class cRetainageAcctID : PX.Data.BQL.BqlInt.Field<cRetainageAcctID> { }

		/// <inheritdoc cref="CR.Location.CRetainageAcctID"/>
		[PXDBInt()]
		[PXUIField(DisplayName = "Retainage Receivable Account")]
		public virtual int? CRetainageAcctID { get; set; }
		#endregion
		#region CRetainageSubID
		public abstract class cRetainageSubID : PX.Data.BQL.BqlInt.Field<cRetainageSubID> { }

		/// <inheritdoc cref="CR.Location.CRetainageSubID"/>
		[PXDBInt()]
		[PXUIField(DisplayName = "Retainage Receivable Sub.")]
		public virtual int? CRetainageSubID { get; set; }
		#endregion
		#region CFreightAcctID
		public abstract class cFreightAcctID : PX.Data.BQL.BqlInt.Field<cFreightAcctID> { }

		/// <inheritdoc cref="CR.Location.CFreightAcctID"/>
		[PXDBInt()]
		[PXUIField(DisplayName = "Freight Account")]
		public virtual Int32? CFreightAcctID { get; set; }
		#endregion
		#region CFreightSubID
		public abstract class cFreightSubID : PX.Data.BQL.BqlInt.Field<cFreightSubID> { }

		/// <inheritdoc cref="CR.Location.CFreightSubID"/>
		[PXDBInt()]
		[PXUIField(DisplayName = "Freight Sub.")]
		public virtual Int32? CFreightSubID { get; set; }
		#endregion
		#region CShipComplete
		public abstract class cShipComplete : PX.Data.BQL.BqlString.Field<cShipComplete> { }

		/// <inheritdoc cref="CR.Location.CShipComplete"/>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(SOShipComplete.CancelRemainder)]
		[SOShipComplete.List()]
		[PXUIField(DisplayName = "Shipping Rule")]
		public virtual String CShipComplete { get; set; }
		#endregion
		#region COrderPriority
		public abstract class cOrderPriority : PX.Data.BQL.BqlShort.Field<cOrderPriority> { }

		/// <inheritdoc cref="CR.Location.COrderPriority"/>
		[PXDBShort()]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "Order Priority")]
		public virtual Int16? COrderPriority { get; set; }
		#endregion
		#region CCalendarID
		public abstract class cCalendarID : PX.Data.BQL.BqlString.Field<cCalendarID> { }

		/// <inheritdoc cref="CR.Location.CCalendarID"/>
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Calendar")]
		public virtual String CCalendarID { get; set; }
		#endregion
		#region CDefProject
		public abstract class cDefProjectID : PX.Data.BQL.BqlInt.Field<cDefProjectID> { }

		/// <inheritdoc cref="CR.Location.CDefProjectID"/>
		[PXDBInt]
		[PXUIField(DisplayName = "Default Project")]
		public virtual Int32? CDefProjectID { get; set; }
		#endregion
		#region CARAccountLocationID
		public abstract class cARAccountLocationID : PX.Data.BQL.BqlInt.Field<cARAccountLocationID> { }

		/// <inheritdoc cref="CR.Location.CARAccountLocationID"/>
		[PXDBInt()]
		[PXDefault()]
		public virtual Int32? CARAccountLocationID { get; set; }
		#endregion
		#region CARAccountID
		public abstract class cARAccountID : PX.Data.BQL.BqlInt.Field<cARAccountID> { }

		/// <inheritdoc cref="CR.Location.CARAccountID"/>
		[PXDBInt()]
		[PXUIField(DisplayName = "AR Account")]
		public virtual Int32? CARAccountID { get; set; }
		#endregion
		#region CARSubID
		public abstract class cARSubID : PX.Data.BQL.BqlInt.Field<cARSubID> { }

		/// <inheritdoc cref="CR.Location.CARSubID"/>
		[PXDBInt()]
		[PXUIField(DisplayName = "AR Sub.")]
		public virtual Int32? CARSubID { get; set; }
		#endregion
		#region IsARAccountSameAsMain
		public abstract class isARAccountSameAsMain : PX.Data.BQL.BqlBool.Field<isARAccountSameAsMain> { }

		/// <inheritdoc cref="CR.Location.IsARAccountSameAsMain"/>
		[PXBool()]
		[PXUIField(DisplayName = "Same As Default Location's")]
		[PXFormula(typeof(Switch<Case<Where<locationID, Equal<cARAccountLocationID>>, False>, True>))]
		public virtual bool? IsARAccountSameAsMain { get; set; }
		#endregion



		// Vendor Location Properties
		#region VTaxZoneID
		public abstract class vTaxZoneID : PX.Data.BQL.BqlString.Field<vTaxZoneID> { }

		/// <inheritdoc cref="CR.Location.VTaxZoneID"/>
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Zone")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String VTaxZoneID { get; set; }
		#endregion
		#region VTaxCalcMode
		public abstract class vTaxCalcMode : PX.Data.BQL.BqlString.Field<vTaxCalcMode> { }

		/// <inheritdoc cref="CR.Location.VTaxCalcMode"/>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(TaxCalculationMode.TaxSetting, typeof(Search<VendorClass.taxCalcMode, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>>))]
		[TaxCalculationMode.List]
		[PXUIField(DisplayName = "Tax Calculation Mode")]
		public virtual string VTaxCalcMode { get; set; }
		#endregion
		#region VCarrierID
		public abstract class vCarrierID : PX.Data.BQL.BqlString.Field<vCarrierID> { }

		/// <inheritdoc cref="CR.Location.VCarrierID"/>
		[PXUIField(DisplayName = "Ship Via")]
		[PXDBString(15, IsUnicode = true, InputMask = ">aaaaaaaaaaaaaaa")]
		public virtual String VCarrierID { get; set; }
		#endregion
		#region VShipTermsID
		public abstract class vShipTermsID : PX.Data.BQL.BqlString.Field<vShipTermsID> { }

		/// <inheritdoc cref="CR.Location.VShipTermsID"/>
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Shipping Terms")]
		public virtual String VShipTermsID { get; set; }
		#endregion
		#region VFOBPointID
		public abstract class vFOBPointID : PX.Data.BQL.BqlString.Field<vFOBPointID> { }

		/// <inheritdoc cref="CR.Location.VFOBPointID"/>
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "FOB Point")]
		public virtual String VFOBPointID { get; set; }
		#endregion
		#region VLeadTime
		public abstract class vLeadTime : PX.Data.BQL.BqlShort.Field<vLeadTime> { }

		/// <inheritdoc cref="CR.Location.VLeadTime"/>
		[PXDBShort(MinValue = 0, MaxValue = 100000)]
		[PXUIField(DisplayName = CR.Messages.LeadTimeDays)]
		public virtual Int16? VLeadTime { get; set; }
		#endregion
		#region VBranchID
		public abstract class vBranchID : PX.Data.BQL.BqlInt.Field<vBranchID> { }

		/// <inheritdoc cref="CR.Location.VBranchID"/>
		[PXDBInt()]
		[PXUIField(DisplayName = "Receiving Branch")]
		public virtual Int32? VBranchID { get; set; }
		#endregion
		#region VExpenseAcctID
		public abstract class vExpenseAcctID : PX.Data.BQL.BqlInt.Field<vExpenseAcctID> { }

		/// <inheritdoc cref="CR.Location.VExpenseAcctID"/>
		[PXDBInt()]
		[PXUIField(DisplayName = "Expense Account")]
		public virtual Int32? VExpenseAcctID { get; set; }
		#endregion
		#region VExpenseSubID
		public abstract class vExpenseSubID : PX.Data.BQL.BqlInt.Field<vExpenseSubID> { }

		/// <inheritdoc cref="CR.Location.VExpenseSubID"/>
		[PXDBInt()]
		[PXUIField(DisplayName = "Expense Sub.")]
		public virtual Int32? VExpenseSubID { get; set; }
		#endregion
		#region VRetainageAcctID
		public abstract class vRetainageAcctID : PX.Data.BQL.BqlInt.Field<vRetainageAcctID> { }

		/// <inheritdoc cref="CR.Location.VRetainageAcctID"/>
		[PXDBInt()]
		[PXUIField(DisplayName = "Retainage Payable Account")]
		public virtual int? VRetainageAcctID { get; set; }
		#endregion
		#region VRetainageSubID
		public abstract class vRetainageSubID : PX.Data.BQL.BqlInt.Field<vRetainageSubID> { }

		/// <inheritdoc cref="CR.Location.VRetainageSubID"/>
		[PXDBInt()]
		[PXUIField(DisplayName = "Retainage Payable Sub.")]
		public virtual int? VRetainageSubID { get; set; }
		#endregion
		#region VFreightAcctID
		public abstract class vFreightAcctID : PX.Data.BQL.BqlInt.Field<vFreightAcctID> { }

		/// <inheritdoc cref="CR.Location.VFreightAcctID"/>
		[PXDBInt()]
		[PXUIField(DisplayName = "Freight Account")]
		public virtual Int32? VFreightAcctID { get; set; }
		#endregion
		#region VFreightSubID
		public abstract class vFreightSubID : PX.Data.BQL.BqlInt.Field<vFreightSubID> { }

		/// <inheritdoc cref="CR.Location.VFreightSubID"/>
		[PXDBInt()]
		[PXUIField(DisplayName = "Freight Sub.")]
		public virtual Int32? VFreightSubID { get; set; }
		#endregion
		#region VDiscountAcctID
		public abstract class vDiscountAcctID : PX.Data.BQL.BqlInt.Field<vDiscountAcctID> { }

		/// <inheritdoc cref="CR.Location.VDiscountAcctID"/>
		[PXDBInt()]
		[PXUIField(DisplayName = "Discount Account")]
		public virtual Int32? VDiscountAcctID { get; set; }
		#endregion
		#region VDiscountSubID
		public abstract class vDiscountSubID : PX.Data.BQL.BqlInt.Field<vDiscountSubID> { }

		/// <inheritdoc cref="CR.Location.VDiscountSubID"/>
		[PXDBInt()]
		[PXUIField(DisplayName = "Discount Sub.")]
		public virtual Int32? VDiscountSubID { get; set; }
		#endregion
		#region VRcptQtyMin
		public abstract class vRcptQtyMin : PX.Data.BQL.BqlDecimal.Field<vRcptQtyMin> { }

		/// <inheritdoc cref="CR.Location.VRcptQtyMin"/>
		[PXDBDecimal(2, MinValue = 0.0, MaxValue = 999.0)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Min. Receipt (%)")]
		public virtual Decimal? VRcptQtyMin { get; set; }
		#endregion
		#region VRcptQtyMax
		public abstract class vRcptQtyMax : PX.Data.BQL.BqlDecimal.Field<vRcptQtyMax> { }

		/// <inheritdoc cref="CR.Location.VRcptQtyMax"/>
		[PXDBDecimal(2, MinValue = 0.0, MaxValue = 999.0)]
		[PXDefault(TypeCode.Decimal, "100.0")]
		[PXUIField(DisplayName = "Max. Receipt (%)")]
		public virtual Decimal? VRcptQtyMax { get; set; }
		#endregion
		#region VRcptQtyThreshold
		public abstract class vRcptQtyThreshold : PX.Data.BQL.BqlDecimal.Field<vRcptQtyThreshold> { }

		/// <inheritdoc cref="CR.Location.VRcptQtyThreshold"/>
		[PXDBDecimal(2, MinValue = 0.0, MaxValue = 999.0)]
		[PXDefault(TypeCode.Decimal, "100.0")]
		[PXUIField(DisplayName = "Threshold Receipt (%)")]
		public virtual Decimal? VRcptQtyThreshold { get; set; }
		#endregion
		#region VRcptQtyAction
		public abstract class vRcptQtyAction : PX.Data.BQL.BqlString.Field<vRcptQtyAction> { }

		/// <inheritdoc cref="CR.Location.VRcptQtyAction"/>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(POReceiptQtyAction.AcceptButWarn)]
		[POReceiptQtyAction.List()]
		[PXUIField(DisplayName = "Receipt Action")]
		public virtual String VRcptQtyAction { get; set; }
		#endregion
		#region VSiteID
		public abstract class vSiteID : PX.Data.BQL.BqlInt.Field<vSiteID> { }

		/// <inheritdoc cref="CR.Location.VSiteID"/>
		[PXDBInt()]
		[PXUIField(DisplayName = "Warehouse", Visibility = PXUIVisibility.Visible, FieldClass = SiteAttribute.DimensionName)]
		[PXDimensionSelector(SiteAttribute.DimensionName, typeof(INSite.siteID), typeof(INSite.siteCD), DescriptionField = typeof(INSite.descr))]
		[PXRestrictor(typeof(Where<INSite.active, Equal<True>>), IN.Messages.InactiveWarehouse, typeof(INSite.siteCD))]
		[PXRestrictor(typeof(Where<INSite.siteID, NotEqual<SiteAttribute.transitSiteID>>), IN.Messages.TransitSiteIsNotAvailable)]
		public virtual Int32? VSiteID { get; set; }
		#endregion
		#region VSiteIDIsNull
		public abstract class vSiteIDIsNull : PX.Data.BQL.BqlShort.Field<vSiteIDIsNull> { }

		/// <inheritdoc cref="CR.Location.VSiteIDIsNull"/>
		[PXShort()]
		[PXDBCalced(typeof(Switch<Case<Where<Location.vSiteID, IsNull>, shortMax>, short0>), typeof(short))]
		public virtual Int16? VSiteIDIsNull { get; set; }
		#endregion
		#region VPrintOrder
		public abstract class vPrintOrder : PX.Data.BQL.BqlBool.Field<vPrintOrder> { }

		/// <inheritdoc cref="CR.Location.VPrintOrder"/>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Print Order")]
		public virtual bool? VPrintOrder { get; set; }
		#endregion
		#region VEmailOrder
		public abstract class vEmailOrder : PX.Data.BQL.BqlBool.Field<vEmailOrder> { }

		/// <inheritdoc cref="CR.Location.VEmailOrder"/>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Email Order")]
		public virtual bool? VEmailOrder { get; set; }
		#endregion
		#region VDefProjectID
		public abstract class vDefProjectID : PX.Data.BQL.BqlInt.Field<vDefProjectID> { }

		/// <inheritdoc cref="CR.Location.VDefProjectID"/>
		[PXDBInt]
		[PXUIField(DisplayName = "Default Project")]
		public virtual Int32? VDefProjectID { get; set; }
		#endregion
		#region VAPAccountLocationID
		public abstract class vAPAccountLocationID : PX.Data.BQL.BqlInt.Field<vAPAccountLocationID> { }

		/// <inheritdoc cref="CR.Location.VAPAccountLocationID"/>
		[PXDBInt()]
		[PXDefault()]
		public virtual Int32? VAPAccountLocationID { get; set; }
		#endregion
		#region VAPAccountID
		public abstract class vAPAccountID : PX.Data.BQL.BqlInt.Field<vAPAccountID> { }

		/// <inheritdoc cref="CR.Location.VAPAccountID"/>
		[PXDBInt()]
		[PXUIField(DisplayName = "AP Account")]
		public virtual Int32? VAPAccountID { get; set; }
		#endregion
		#region VAPSubID
		public abstract class vAPSubID : PX.Data.BQL.BqlInt.Field<vAPSubID> { }

		/// <inheritdoc cref="CR.Location.VAPSubID"/>
		[PXDBInt()]
		[PXUIField(DisplayName = "AP Sub.")]
		public virtual Int32? VAPSubID { get; set; }
		#endregion
		#region VPaymentInfoLocationID
		public abstract class vPaymentInfoLocationID : PX.Data.BQL.BqlInt.Field<vPaymentInfoLocationID> { }

		/// <inheritdoc cref="CR.Location.VPaymentInfoLocationID"/>
		[PXDBInt()]
		[PXDefault()]
		public virtual Int32? VPaymentInfoLocationID { get; set; }
		#endregion
		#region OverrideRemitAddress
		public abstract class overrideRemitAddress : PX.Data.BQL.BqlBool.Field<overrideRemitAddress> { }

		/// <inheritdoc cref="CR.Location.OverrideRemitAddress"/>
		[PXBool]
		[PXUIField(DisplayName = "Override")]
		public virtual bool? OverrideRemitAddress { get; set; }
		#endregion
		#region IsRemitAddressSameAsMain
		[Obsolete("Use OverrideRemitAddress instead")]
		public abstract class isRemitAddressSameAsMain : PX.Data.BQL.BqlBool.Field<isRemitAddressSameAsMain> { }
		protected bool? _IsRemitAddressSameAsMain;

		/// <inheritdoc cref="CR.Location.IsRemitAddressSameAsMain"/>
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

		/// <inheritdoc cref="CR.Location.VRemitAddressID"/>
		[PXDBInt()]
		[PXForeignReference(typeof(Field<Location.vRemitAddressID>.IsRelatedTo<Address.addressID>))]
		public virtual Int32? VRemitAddressID { get; set; }
		#endregion
		#region OverrideRemitContact
		public abstract class overrideRemitContact : PX.Data.BQL.BqlBool.Field<overrideRemitContact> { }

		/// <inheritdoc cref="CR.Location.OverrideRemitContact"/>
		[PXBool]
		[PXUIField(DisplayName = "Override")]
		public virtual bool? OverrideRemitContact { get; set; }
		#endregion
		#region IsRemitContactSameAsMain
		[Obsolete("Use OverrideRemitContact instead")]
		public abstract class isRemitContactSameAsMain : PX.Data.BQL.BqlBool.Field<isRemitContactSameAsMain> { }
		protected bool? _IsRemitContactSameAsMain;

		/// <inheritdoc cref="CR.Location.IsRemitContactSameAsMain"/>
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

		/// <inheritdoc cref="CR.Location.VRemitContactID"/>
		[PXDBInt()]
		[PXForeignReference(typeof(Field<Location.vRemitContactID>.IsRelatedTo<Contact.contactID>))]
		public virtual Int32? VRemitContactID { get; set; }
		#endregion
		#region VPaymentMethodID
		public abstract class vPaymentMethodID : PX.Data.BQL.BqlString.Field<vPaymentMethodID> { }

		/// <inheritdoc cref="CR.Location.VPaymentMethodID"/>
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Payment Method")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String VPaymentMethodID { get; set; }
		#endregion
		#region VCashAccountID
		public abstract class vCashAccountID : PX.Data.BQL.BqlInt.Field<vCashAccountID> { }

		/// <inheritdoc cref="CR.Location.VCashAccountID"/>
		[PXDBInt()]
		[PXUIField(DisplayName = "Cash Account")]
		public virtual Int32? VCashAccountID { get; set; }
		#endregion
		#region VPaymentLeadTime
		public abstract class vPaymentLeadTime : PX.Data.BQL.BqlShort.Field<vPaymentLeadTime> { }

		/// <inheritdoc cref="CR.Location.VPaymentLeadTime"/>
		[PXDBShort(MinValue = 0, MaxValue = 3660)]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "Payment Lead Time (Days)")]
		public Int16? VPaymentLeadTime { get; set; }
		#endregion
		#region VPaymentByType
		public abstract class vPaymentByType : PX.Data.BQL.BqlInt.Field<vPaymentByType> { }

		/// <inheritdoc cref="CR.Location.VPaymentByType"/>
		[PXDBInt()]
		[PXUIField(DisplayName = "Payment By")]
		[PXDefault(APPaymentBy.DueDate)]
		[APPaymentBy.List]
		public int? VPaymentByType { get; set; }
		#endregion
		#region VSeparateCheck
		public abstract class vSeparateCheck : PX.Data.BQL.BqlBool.Field<vSeparateCheck> { }

		/// <inheritdoc cref="CR.Location.VSeparateCheck"/>
		[PXDBBool()]
		[PXUIField(DisplayName = "Pay Separately")]
		[PXDefault(false)]
		public virtual Boolean? VSeparateCheck { get; set; }
		#endregion
		#region VPrepaymentPct
		public abstract class vPrepaymentPct : Data.BQL.BqlDecimal.Field<vPrepaymentPct> { }

		/// <inheritdoc cref="CR.Location.VPrepaymentPct"/>
		[PXDBDecimal(6)]
		[PXUIField(DisplayName = "Prepayment Percent")]
		[PXDefault(TypeCode.Decimal, "100.0")]
		public virtual decimal? VPrepaymentPct { get; set; }
		#endregion
		#region VAllowAPBillBeforeReceipt
		public abstract class vAllowAPBillBeforeReceipt : PX.Data.BQL.BqlBool.Field<vAllowAPBillBeforeReceipt> { }

		/// <inheritdoc cref="CR.Location.VAllowAPBillBeforeReceipt"/>
		[PXDBBool]
		[PXUIField(DisplayName = "Allow AP Bill Before Receipt")]
		[PXDefault(false)]
		public virtual bool? VAllowAPBillBeforeReceipt { get; set; }
		#endregion
		#region IsAPAccountSameAsMain
		public abstract class isAPAccountSameAsMain : PX.Data.BQL.BqlBool.Field<isAPAccountSameAsMain> { }

		/// <inheritdoc cref="CR.Location.IsAPAccountSameAsMain"/>
		[PXBool()]
		[PXUIField(DisplayName = "Same As Default Location's")]
		[PXFormula(typeof(Switch<Case<Where<locationID, Equal<vAPAccountLocationID>>, False>, True>))]
		public virtual bool? IsAPAccountSameAsMain { get; set; }
		#endregion
		#region IsAPPaymentInfoSameAsMain
		public abstract class isAPPaymentInfoSameAsMain : PX.Data.BQL.BqlBool.Field<isAPPaymentInfoSameAsMain> { }

		/// <inheritdoc cref="CR.Location.IsAPPaymentInfoSameAsMain"/>
		[PXBool()]
		[PXUIField(DisplayName = "Same As Default Location's")]
		[PXFormula(typeof(Switch<Case<Where<locationID, Equal<vPaymentInfoLocationID>>, False>, True>))]
		public virtual bool? IsAPPaymentInfoSameAsMain { get; set; }
		#endregion

		// Company Location Properties
		#region CMPSalesSubID
		public abstract class cMPSalesSubID : PX.Data.BQL.BqlInt.Field<cMPSalesSubID> { }

		/// <inheritdoc cref="CR.Location.CMPSalesSubID"/>
		[PXDBInt()]
		[PXUIField(DisplayName = "Sales Sub.")]
		public virtual Int32? CMPSalesSubID { get; set; }
		#endregion
		#region CMPExpenseSubID
		public abstract class cMPExpenseSubID : PX.Data.BQL.BqlInt.Field<cMPExpenseSubID> { }

		/// <inheritdoc cref="CR.Location.CMPExpenseSubID"/>
		[PXDBInt()]
		[PXUIField(DisplayName = "Expense Sub.")]
		public virtual Int32? CMPExpenseSubID { get; set; }
		#endregion
		#region CMPFreightSubID
		public abstract class cMPFreightSubID : PX.Data.BQL.BqlInt.Field<cMPFreightSubID> { }

		/// <inheritdoc cref="CR.Location.CMPFreightSubID"/>
		[PXDBInt()]
		[PXUIField(DisplayName = "Freight Sub.")]
		public virtual Int32? CMPFreightSubID { get; set; }
		#endregion
		#region CMPDiscountSubID
		public abstract class cMPDiscountSubID : PX.Data.BQL.BqlInt.Field<cMPDiscountSubID> { }

		/// <inheritdoc cref="CR.Location.CMPDiscountSubID"/>
		[PXDBInt()]
		[PXUIField(DisplayName = "Discount Sub.")]
		public virtual Int32? CMPDiscountSubID { get; set; }
		#endregion
		#region CMPGainLossSubID
		public abstract class cMPGainLossSubID : PX.Data.BQL.BqlInt.Field<cMPGainLossSubID> { }

		/// <inheritdoc cref="CR.Location.CMPGainLossSubID"/>
		[PXDBInt()]
		[PXUIField(DisplayName = "Currency Gain/Loss Sub.")]
		public virtual Int32? CMPGainLossSubID { get; set; }
		#endregion
		#region CMPSiteID
		public abstract class cMPSiteID : PX.Data.BQL.BqlInt.Field<cMPSiteID> { }

		/// <inheritdoc cref="CR.Location.CMPSiteID"/>
		[PXDBInt()]
		[PXUIField(DisplayName = "Warehouse")]
		public virtual Int32? CMPSiteID { get; set; }
		#endregion

		// Audit Fields
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		[PXDBTimestamp()]
		public virtual Byte[] tstamp { get; set; }
		#endregion
	}
}
