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
using System.Collections;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.Common;
using PX.Objects.GL;
using PX.Objects.CS;
using CRLocation = PX.Objects.CR.Standalone.Location;
using Branch = PX.Objects.GL.Branch;
using PX.SM;
using PX.Objects.GL.Helpers;
using PX.Objects.TX;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CR.Extensions.Relational;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.CR
{
	[PXProjection(typeof(Select<CRLocation>), Persistent = false)]
    [PXCacheName(Messages.LocationARAccountSub)]
    [Serializable]
	public partial class LocationARAccountSub : IBqlTable
	{
		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		protected int? _BAccountID;
		[PXDBInt(BqlField = typeof(CRLocation.bAccountID), IsKey = true)]
		public virtual int? BAccountID
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
		protected int? _LocationID;
		[PXDBInt(BqlField = typeof(CRLocation.locationID), IsKey = true)]
		public virtual int? LocationID
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
		#region CARAccountLocationID
		public abstract class cARAccountLocationID : PX.Data.BQL.BqlInt.Field<cARAccountLocationID> { }
		protected Int32? _CARAccountLocationID;
		[PXDBInt(BqlField = typeof(CRLocation.cARAccountLocationID))]
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
		[Account(BqlField = typeof(CRLocation.cARAccountID), DisplayName = "AR Account", DescriptionField = typeof(Account.description), Required = true, ControlAccountForModule = ControlAccountModule.AR)]
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
		[SubAccount(typeof(LocationARAccountSub.cARAccountID), BqlField = typeof(CRLocation.cARSubID), DisplayName = "AR Sub.", DescriptionField = typeof(Sub.description), Required = true)]
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
		#region CRetainageAcctID
		public abstract class cRetainageAcctID : PX.Data.BQL.BqlInt.Field<cRetainageAcctID> { }

		[Account(BqlField = typeof(CRLocation.cRetainageAcctID),
			DisplayName = "Retainage Receivable Account",
			DescriptionField = typeof(Account.description),
			Required = true,
			ControlAccountForModule = ControlAccountModule.AR)]
		public virtual int? CRetainageAcctID
		{
			get;
			set;
		}
		#endregion
		#region CRetainageSubID
		public abstract class cRetainageSubID : PX.Data.BQL.BqlInt.Field<cRetainageSubID> { }

		[SubAccount(typeof(LocationARAccountSub.cRetainageAcctID),
			BqlField = typeof(CRLocation.cRetainageSubID),
			DisplayName = "Retainage Receivable Sub.",
			DescriptionField = typeof(Sub.description),
			Required = true)]
		[PXReferentialIntegrityCheck(CheckPoint = CheckPoint.BeforePersisting)]
		[PXForeignReference(typeof(Field<cRetainageSubID>.IsRelatedTo<Sub.subID>))]
		public virtual int? CRetainageSubID
		{
			get;
			set;
		}
		#endregion
	}

	public abstract class LocationMaint : PXGraph<LocationMaint>
	{
		#region Buttons

		public PXSave<Location> Save;
		public PXAction<Location> cancel;
		public PXInsert<Location> Insert;
		public PXDelete<Location> Delete;
		public PXFirst<Location> First;
		public PXPrevious<Location> previous;
		public PXNext<Location> next;
		public PXLast<Location> Last;

		public PXAction<Location> viewOnMap;
		public PXAction<Location> validateAddresses;

		#endregion


		#region ButtonDelegates

		[PXUIField(DisplayName = ActionsMessages.Cancel, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXCancelButton]
		protected virtual IEnumerable Cancel(PXAdapter adapter)
		{
			int? acctid = Location.Current != null ? Location.Current.BAccountID : null;
			foreach (object loc in (new PXCancel<Location>(this, "Cancel")).Press(adapter))
			{
				Location location = PXResult.Unwrap<Location>(loc);

				if (!IsImport && Location.Cache.GetStatus(location) == PXEntryStatus.Inserted
						&& (acctid != location.BAccountID || string.IsNullOrEmpty(location.LocationCD)))
				{
					foreach (object first in First.Press(adapter))
					{
						return new object[] { first };
					}
					location.LocationCD = null;
					return new object[] { location };
				}
				else
				{
					return new object[] { location };
				}
			}
			return new object[0];
		}

		[PXUIField(DisplayName = ActionsMessages.Previous, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXPreviousButton]
		protected virtual IEnumerable Previous(PXAdapter adapter)
		{
			foreach (object loc in (new PXPrevious<Location>(this, "Prev")).Press(adapter))
			{
				Location location = PXResult.Unwrap<Location>(loc);

				if (Location.Cache.GetStatus(location) == PXEntryStatus.Inserted)
				{
					return Last.Press(adapter);
				}
				else
				{
					return new object[] { loc };
				}
			}
			return new object[0];
		}

		[PXUIField(DisplayName = ActionsMessages.Next, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXNextButton]
		protected virtual IEnumerable Next(PXAdapter adapter)
		{
			foreach (object loc in (new PXNext<Location>(this, "Next")).Press(adapter))
			{
				Location location = PXResult.Unwrap<Location>(loc);

				if (Location.Cache.GetStatus(location) == PXEntryStatus.Inserted)
				{
					return First.Press(adapter);
				}
				else
				{
					return new object[] { loc };
				}
			}
			return new object[0];
		}

		[PXUIField(DisplayName = Messages.ViewOnMap, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton()]
		public virtual IEnumerable ViewOnMap(PXAdapter adapter)
		{

			BAccountUtility.ViewOnMap(this.Address.Current);
			return adapter.Get();
		}

		[PXUIField(DisplayName = CS.Messages.ValidateAddresses, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, FieldClass = CS.Messages.ValidateAddress)]
		[PXButton]
		public virtual IEnumerable ValidateAddresses(PXAdapter adapter)
		{
			Location primary = this.LocationCurrent.Current;
			if (primary != null)
			{
				BAccount acct = BAccountUtility.FindAccount(this, primary.BAccountID);
				bool isOverrideAddress = (acct != null && acct.DefAddressID != primary.DefAddressID);
				Address address = this.Address.Current;
				if (address != null && isOverrideAddress == true && address.IsValidated == false)
				{
					PXAddressValidator.Validate<Address>(this, address, true, true);
				}
			}
			return adapter.Get();
		}

		#endregion

		#region Public Selects

		public PXSelect<Location, Where<Location.bAccountID, Equal<Optional<Location.bAccountID>>>> Location;

		public PXSelect<BAccount> BusinessAccount;

		public PXSelect<Location, Where<Location.bAccountID, Equal<Current<Location.bAccountID>>, And<Location.locationID, Equal<Current<Location.locationID>>>>> LocationCurrent;

		public PXSelect<Location, Where<Location.bAccountID, Equal<Current<Location.bAccountID>>>> OtherLocations;

		public PXSelect<Address, Where<Address.bAccountID, Equal<Current<Location.bAccountID>>,
										And<Address.addressID, Equal<Current<Location.defAddressID>>>>> Address;
		public PXSelect<Contact, Where<Contact.bAccountID, Equal<Current<Location.bAccountID>>,
										And<Contact.contactID, Equal<Current<Location.defContactID>>>>> Contact;

		[PXFilterable]
		[PXViewDetailsButton(typeof(Location))]
		[PXViewDetailsButton(typeof(Location),
			typeof(Select<Contact,
				Where<Contact.contactID, Equal<Current<CROpportunity.contactID>>>>))]
		public PXSelectJoin<CROpportunity,
				LeftJoin<Contact, On<Contact.contactID, Equal<CROpportunity.contactID>>,
					LeftJoin<CROpportunityProbability, On<CROpportunityProbability.stageCode, Equal<CROpportunity.stageID>>>>,
				Where<CROpportunity.bAccountID, Equal<Current<Location.bAccountID>>,
					And<CROpportunity.locationID, Equal<Current<Location.locationID>>>>>
			Opportunities;

		[PXFilterable]
		[PXViewDetailsButton(typeof(Location))]
		[PXViewDetailsButton(typeof(Location),
			typeof(Select<Contact,
				Where<Contact.contactID, Equal<Current<CRCase.contactID>>>>))]
		public PXSelect<CRCase,
				Where<CRCase.customerID, Equal<Current<Location.bAccountID>>,
					And<CRCase.locationID, Equal<Current<Location.locationID>>>>>
			Cases;

		public PXSelect<LocationARAccountSub, Where<LocationARAccountSub.bAccountID, Equal<Current<Location.bAccountID>>, And<LocationARAccountSub.locationID, Equal<Current<Location.cARAccountLocationID>>>>> ARAccountSubLocation;

		[PXHidden]
		public PXSelect<CustSalesPeople> Salesperson;

		[PXViewName(Messages.Address)]
		public SelectFrom<Address>
			.Where<
				Address.addressID.IsEqual<Location.defAddressID.FromCurrent>>
			.View
			AddressCurrent;

		public PXSetup<GL.Branch> Company;

		public LocationMaint()
		{
			Location.Join<LeftJoin<BAccount, On<BAccount.bAccountID, Equal<Location.bAccountID>>>>();

			Location.WhereAnd<Where<
				BAccount.bAccountID, Equal<Location.bAccountID>,
				And<BAccount.bAccountID, IsNotNull,
				And<Match<BAccount, Current<AccessInfo.userName>>>>>>();

            PXUIFieldAttribute.SetDisplayName<Contact.salutation>(Contact.Cache, CR.Messages.Attention);

			LocationValidator = new LocationValidator();

			if (this.Company.Current.BAccountID.HasValue == false)
			{
				throw new PXSetupNotEnteredException(ErrorMessages.SetupNotEntered, typeof(Branch), PXMessages.LocalizeNoPrefix(CS.Messages.BranchMaint));
			}
			PXUIFieldAttribute.SetDisplayName<Location.locationCD>(Location.Cache, Messages.LocationID);
			PXUIFieldAttribute.SetDisplayName<Location.cLeadTime>(LocationCurrent.Cache, Messages.LeadTimeDays);

			var bAccountCache = Caches[typeof(BAccount)];
			PXUIFieldAttribute.SetDisplayName<BAccount.acctCD>(bAccountCache, Messages.BAccountCD);
			PXUIFieldAttribute.SetDisplayName<BAccount.acctName>(bAccountCache, Messages.BAccountName);
		}

		#endregion

		protected LocationValidator LocationValidator;

		#region Main Record Events

		[PXDBInt(IsKey = true)]
		[PXDefault(typeof(Location.bAccountID))]
		[PXUIField(DisplayName = "Business Account", TabOrder = 0)]
		[PXDimensionSelector("BIZACCT", typeof(Search2<BAccount.bAccountID,
			LeftJoin<Contact, 
				On<Contact.bAccountID, Equal<BAccount.bAccountID>, And<Contact.contactID, Equal<BAccount.defContactID>>>,
			LeftJoin<Address, 
				On<Address.bAccountID, Equal<BAccount.bAccountID>, And<Address.addressID, Equal<BAccount.defAddressID>>>>>,
			Where<
				BAccount.type, Equal<BAccountType.customerType>,
				Or<BAccount.type, Equal<BAccountType.prospectType>,
				Or<BAccount.type, Equal<BAccountType.vendorType>,
				Or<BAccount.type, Equal<BAccountType.combinedType>>>>>>), 
			substituteKey: typeof(BAccount.acctCD),
			fieldList: new []
			{
				typeof(BAccount.acctCD),
				typeof(BAccount.acctName),
				typeof(BAccount.type),
				typeof(BAccount.classID),
				typeof(BAccount.status),
				typeof(Contact.phone1),
				typeof(Address.city),
				typeof(Address.countryID)
			},
			DescriptionField = typeof(BAccount.acctName))]
		[PXParent(typeof(Select<BAccount,
			Where<BAccount.bAccountID,
			Equal<Current<Location.bAccountID>>>>)
			)]
		protected virtual void _(Events.CacheAttached<Location.bAccountID> e) { }

		[PXCustomizeBaseAttribute(typeof(CS.LocationRawAttribute), nameof(CS.LocationRawAttribute.DescriptionField), typeof(Location.descr))]
		protected virtual void _(Events.CacheAttached<Location.locationCD> e) { }

		protected virtual void Location_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			Location row = e.Row as Location;
			if (row != null)
			{
				BAccount acct = CR.Location.FK.BAccount.FindParent(this, row);

				PXUIFieldAttribute.SetEnabled<CR.Location.isARAccountSameAsMain>(cache, e.Row, acct != null && !object.Equals(acct.DefLocationID, ((Location)e.Row).LocationID));

				bool VendorDetailsVisible = false;
				bool CustomerDetailsVisible = (row.LocType == LocTypeList.CustomerLoc || row.LocType == LocTypeList.CombinedLoc);
				bool CompanyDetailsVisible = (row.LocType == LocTypeList.CompanyLoc);

				if (row.LocType == LocTypeList.VendorLoc || row.LocType == LocTypeList.CombinedLoc)
				{
					VendorDetailsVisible = true;

					var vendor = VendorMaint.FindByID(this, row.BAccountID);
					if (vendor != null)
					{
						bool expenseAcctFieldsShouldBeRequired = vendor.TaxAgency == true;

						PXUIFieldAttribute.SetRequired<Location.vExpenseAcctID>(cache, expenseAcctFieldsShouldBeRequired);
						PXUIFieldAttribute.SetRequired<Location.vExpenseSubID>(cache, expenseAcctFieldsShouldBeRequired);
					}
				}

				PXUIFieldAttribute.SetEnabled<Location.status>(cache, null, row.IsDefault != true);
				PXUIFieldAttribute.SetEnabled<Location.vTaxZoneID>(cache, null, VendorDetailsVisible || CompanyDetailsVisible);
				PXUIFieldAttribute.SetEnabled<Location.vExpenseAcctID>(cache, null, VendorDetailsVisible);
				PXUIFieldAttribute.SetEnabled<Location.vExpenseSubID>(cache, null, VendorDetailsVisible);
				PXUIFieldAttribute.SetEnabled<Location.vRetainageAcctID>(cache, null, VendorDetailsVisible);
				PXUIFieldAttribute.SetEnabled<Location.vRetainageSubID>(cache, null, VendorDetailsVisible);
				PXUIFieldAttribute.SetEnabled<Location.vDiscountAcctID>(cache, null, VendorDetailsVisible);
				PXUIFieldAttribute.SetEnabled<Location.vDiscountSubID>(cache, null, VendorDetailsVisible);
				PXUIFieldAttribute.SetEnabled<Location.vLeadTime>(cache, null, VendorDetailsVisible);
				PXUIFieldAttribute.SetEnabled<Location.vBranchID>(cache, null, VendorDetailsVisible);
				PXUIFieldAttribute.SetEnabled<Location.vCarrierID>(cache, null, VendorDetailsVisible);
				PXUIFieldAttribute.SetEnabled<Location.vFOBPointID>(cache, null, VendorDetailsVisible);
				PXUIFieldAttribute.SetEnabled<Location.vShipTermsID>(cache, null, VendorDetailsVisible);

				PXUIFieldAttribute.SetEnabled<Location.cTaxZoneID>(cache, null, CustomerDetailsVisible);
				PXUIFieldAttribute.SetEnabled<Location.cAvalaraCustomerUsageType>(cache, null, CustomerDetailsVisible);
				PXUIFieldAttribute.SetEnabled<Location.cSalesAcctID>(cache, null, CustomerDetailsVisible);
				PXUIFieldAttribute.SetEnabled<Location.cSalesSubID>(cache, null, CustomerDetailsVisible);
				PXUIFieldAttribute.SetEnabled<Location.cSalesAcctID>(cache, null, CustomerDetailsVisible);
				PXUIFieldAttribute.SetEnabled<Location.cRetainageAcctID>(cache, null, CustomerDetailsVisible);
				PXUIFieldAttribute.SetEnabled<Location.cRetainageSubID>(cache, null, CustomerDetailsVisible);
				PXUIFieldAttribute.SetEnabled<Location.cDiscountAcctID>(cache, null, CustomerDetailsVisible);
				PXUIFieldAttribute.SetEnabled<Location.cDiscountSubID>(cache, null, CustomerDetailsVisible);
				PXUIFieldAttribute.SetEnabled<Location.cFreightAcctID>(cache, null, CustomerDetailsVisible);
				PXUIFieldAttribute.SetEnabled<Location.cFreightSubID>(cache, null, CustomerDetailsVisible);
				PXUIFieldAttribute.SetEnabled<Location.cBranchID>(cache, null, CustomerDetailsVisible);
				PXUIFieldAttribute.SetEnabled<Location.cCarrierID>(cache, null, CustomerDetailsVisible);
				
				PXUIFieldAttribute.SetEnabled<Location.cFOBPointID>(cache, null, CustomerDetailsVisible);
				PXUIFieldAttribute.SetEnabled<Location.cShipTermsID>(cache, null, CustomerDetailsVisible);

				PXUIFieldAttribute.SetEnabled<Location.cMPSalesSubID>(cache, null, CompanyDetailsVisible);
				PXUIFieldAttribute.SetEnabled<Location.cMPExpenseSubID>(cache, null, CompanyDetailsVisible);

				var isDefault = cache.GetValueOriginal<Location.isDefault>(row) as bool?;
				cache.AdjustUI(row)
					.For<Location.isDefault>(field =>
					{
						field.Enabled = isDefault != true;
					});

				bool isGroundCollectVisible = false;

				if ( row.CCarrierID != null)
				{
					Carrier carrier = Carrier.PK.Find(this, row.CCarrierID);

					if (carrier != null && carrier.IsExternal == true && !string.IsNullOrEmpty(carrier.CarrierPluginID))
					{
						isGroundCollectVisible = CarrierPluginMaint.GetCarrierPluginAttributes(this, carrier.CarrierPluginID).Contains("COLLECT");
					}
				}

				PXUIFieldAttribute.SetVisible<Location.cGroundCollect>(cache, row, isGroundCollectVisible);

				EstablishCTaxZoneRule((field, isRequired) => PXUIFieldAttribute.SetRequired(Location.Cache, field.Name, isRequired));
				EstablishVTaxZoneRule((field, isRequired) => PXUIFieldAttribute.SetRequired(Location.Cache, field.Name, isRequired));
			}
		}

		protected virtual void Location_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
		{
			Location row = e.Row as Location;
			BAccount acct = CR.Location.FK.BAccount.FindParent(this, row);
			if (acct != null)
			{
				if (acct.DefLocationID == row.LocationID)
				{
					throw new PXException(Messages.CannotDeleteDefaultLoc);
				}
			}
		}

		protected virtual void Location_LocType_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			Location row = e.Row as Location;
			if (row != null)
			{
				BAccount acct = CR.Location.FK.BAccount.FindParent(this, row);
				if (acct != null)
				{
					switch (acct.Type)
					{
						case BAccountType.VendorType:
							e.NewValue = LocTypeList.VendorLoc;
							break;
						case BAccountType.CustomerType:
						case BAccountType.EmpCombinedType:
							e.NewValue = LocTypeList.CustomerLoc;
							break;
						case BAccountType.CombinedType:
							e.NewValue = LocTypeList.CombinedLoc;
							break;
						default:
							e.NewValue = LocTypeList.CompanyLoc;
							break;
					}
				}
			}
		}

		protected virtual void Location_DefAddressID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			Location row = e.Row as Location;
			if (row != null)
			{
				BAccount acct = CR.Location.FK.BAccount.FindParent(this, row);
				if (acct != null)
				{
					e.NewValue = acct.DefAddressID;
				}
			}
		}

		protected virtual void Location_DefContactID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			Location row = e.Row as Location;
			if (row != null)
			{
				BAccount acct = CR.Location.FK.BAccount.FindParent(this, row);
				if (acct != null)
				{
					e.NewValue = acct.DefContactID;
				}
			}
		}

		protected virtual void Location_CARAccountLocationID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			Location row = e.Row as Location;
			if (row != null)
			{
				BAccount acct = CR.Location.FK.BAccount.FindParent(this, row);
				if (acct != null)
				{
					e.NewValue = acct.DefLocationID;
				}
			}
		}

		protected virtual void Location_VAPAccountLocationID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			Location row = e.Row as Location;
			if (row != null)
			{
				BAccount acct = CR.Location.FK.BAccount.FindParent(this, row);
				if (acct != null)
				{
					e.NewValue = acct.DefLocationID;
				}
			}
		}

		protected virtual void Location_VPaymentInfoLocationID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			Location row = e.Row as Location;
			if (row != null)
			{
				BAccount acct = CR.Location.FK.BAccount.FindParent(this, row);
				if (acct != null)
				{
					e.NewValue = acct.DefLocationID;
				}
			}
		}

		protected virtual void Location_VRemitAddressID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			Location row = e.Row as Location;
			if (row != null)
			{
				BAccount acct = CR.Location.FK.BAccount.FindParent(this, row);
				if (acct != null)
				{
					e.NewValue = acct.DefAddressID;
				}
			}
		}

		protected virtual void Location_VRemitContactID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			Location row = e.Row as Location;
			if (row != null)
			{
				BAccount acct = CR.Location.FK.BAccount.FindParent(this, row);
				if (acct != null)
				{
					e.NewValue = acct.DefContactID;
				}
			}
		}

		protected virtual void Location_VExpenseAcctID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			Location loc = e.Row as Location;
			if (loc != null && loc.BAccountID != null)
			{
				BAccount acct = CR.Location.FK.BAccount.FindParent(this, loc);
				if (acct != null &&
						 acct.DefLocationID != null &&
						 (acct.Type == BAccountType.VendorType || acct.Type == BAccountType.CombinedType) &&
						 loc.LocationID != acct.DefLocationID)
				{
					Location defLocation = PXSelect<Location>.Search<Location.locationID, Location.bAccountID>(this, acct.DefLocationID, acct.BAccountID);
					if (defLocation != null && defLocation.VExpenseAcctID != null)
					{
						e.NewValue = defLocation.VExpenseAcctID;
						e.Cancel = true;
					}
				}
			}
		}

		protected virtual void Location_VExpenseSubID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			Location loc = e.Row as Location;
			if (loc != null && loc.BAccountID != null)
			{
				BAccount acct = CR.Location.FK.BAccount.FindParent(this, loc);
				if (acct != null &&
						 acct.DefLocationID != null &&
						 (acct.Type == BAccountType.VendorType || acct.Type == BAccountType.CombinedType) &&
						 loc.LocationID != acct.DefLocationID)
				{
					Location defLocation = PXSelect<Location>.Search<Location.locationID, Location.bAccountID>(this, acct.DefLocationID, acct.BAccountID);
					if (defLocation != null && defLocation.VExpenseSubID != null)
					{
						e.NewValue = defLocation.VExpenseSubID;
						e.Cancel = true;
					}
				}
			}
		}

		protected void _(Events.FieldDefaulting<Location, Location.vDiscountAcctID> e)
		{
			Location defLocation = GetDefaultLocation(e.Cache, (Location)e.Row);
			if (defLocation != null && defLocation.VDiscountAcctID != null)
			{
				e.NewValue = defLocation.VDiscountAcctID;
				e.Cancel = true;
			}
		}

		protected void _(Events.FieldDefaulting<Location, Location.vDiscountSubID> e)
		{
			Location defLocation = GetDefaultLocation(e.Cache, (Location)e.Row);
			if (defLocation != null && defLocation.VDiscountSubID != null)
			{
				e.NewValue = defLocation.VDiscountSubID;
				e.Cancel = true;
			}
		}

		protected virtual void Location_CSalesAcctID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			Location loc = e.Row as Location;
			if (loc != null && loc.BAccountID != null)
			{
				BAccount acct = CR.Location.FK.BAccount.FindParent(this, loc);
				if (acct != null &&
						 acct.DefLocationID != null &&
						 (acct.Type == BAccountType.CustomerType || acct.Type == BAccountType.CombinedType) &&
						 loc.LocationID != acct.DefLocationID)
				{
					Location defLocation = PXSelect<Location>.Search<Location.locationID, Location.bAccountID>(this, acct.DefLocationID, acct.BAccountID);
					if (defLocation != null && defLocation.CSalesAcctID != null)
					{
						e.NewValue = defLocation.CSalesAcctID;
						e.Cancel = true;
					}
				}
			}
		}

		protected virtual void Location_CSalesSubID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			Location loc = e.Row as Location;
			if (loc != null && loc.BAccountID != null)
			{
				BAccount acct = CR.Location.FK.BAccount.FindParent(this, loc);
				if (acct != null &&
						 acct.DefLocationID != null &&
						 (acct.Type == BAccountType.CustomerType || acct.Type == BAccountType.CombinedType) &&
						 loc.LocationID != acct.DefLocationID)
				{
					Location defLocation = PXSelect<Location>.Search<Location.locationID, Location.bAccountID>(this, acct.DefLocationID, acct.BAccountID);
					if (defLocation != null && defLocation.CSalesSubID != null)
					{
						e.NewValue = defLocation.CSalesSubID;
						e.Cancel = true;
					}
				}
			}
		}

		protected void _(Events.FieldDefaulting<Location, Location.cARAccountID> e)
		{
			Location defLocation = GetDefaultLocation(e.Cache, (Location)e.Row);
			if (defLocation != null && defLocation.CARAccountID != null)
			{
				e.NewValue = defLocation.CARAccountID;
				e.Cancel = true;
			}
		}

		protected void _(Events.FieldDefaulting<Location, Location.cARSubID> e)
		{
			Location defLocation = GetDefaultLocation(e.Cache, (Location)e.Row);
			if (defLocation != null && defLocation.CARSubID != null)
			{
				e.NewValue = defLocation.CARSubID;
				e.Cancel = true;
			}
		}

		protected void _(Events.FieldDefaulting<Location, Location.cDiscountAcctID> e)
		{
			Location defLocation = GetDefaultLocation(e.Cache, (Location)e.Row);
			if (defLocation != null && defLocation.CDiscountAcctID != null)
			{
				e.NewValue = defLocation.CDiscountAcctID;
				e.Cancel = true;
			}
		}

		protected void _(Events.FieldDefaulting<Location, Location.cDiscountSubID> e)
		{
			Location defLocation = GetDefaultLocation(e.Cache, (Location)e.Row);
			if (defLocation != null && defLocation.CDiscountSubID != null)
			{
				e.NewValue = defLocation.CDiscountSubID;
				e.Cancel = true;
			}
		}

		protected void _(Events.FieldDefaulting<Location, Location.cFreightAcctID> e)
		{
			Location defLocation = GetDefaultLocation(e.Cache, (Location)e.Row);
			if (defLocation != null && defLocation.CFreightAcctID != null)
			{
				e.NewValue = defLocation.CFreightAcctID;
				e.Cancel = true;
			}
		}

		protected void _(Events.FieldDefaulting<Location, Location.cFreightSubID> e)
		{
			Location defLocation = GetDefaultLocation(e.Cache, (Location)e.Row);
			if (defLocation != null && defLocation.CFreightSubID != null)
			{
				e.NewValue = defLocation.CFreightSubID;
				e.Cancel = true;
			}
		}

		protected virtual void Location_CPriceClassID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			Location loc = e.Row as Location;
			if (loc != null && loc.BAccountID != null)
			{
				BAccount acct = CR.Location.FK.BAccount.FindParent(this, loc);
				if (acct != null &&
						 acct.DefLocationID != null &&
						 (acct.Type == BAccountType.CustomerType || acct.Type == BAccountType.CombinedType) &&
						 loc.LocationID != acct.DefLocationID)
				{
					Location defLocation = PXSelect<Location>.Search<Location.locationID, Location.bAccountID>(this, acct.DefLocationID, acct.BAccountID);
					if (defLocation != null && defLocation.CPriceClassID != null)
					{
						e.NewValue = defLocation.CPriceClassID;
						e.Cancel = true;
					}
				}
			}
		}
		protected virtual void Location_VTaxZoneID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			Location loc = e.Row as Location;
			if (loc != null && loc.BAccountID != null)
			{
				BAccount acct = CR.Location.FK.BAccount.FindParent(this, loc);
				if (acct != null &&
						 acct.DefLocationID != null &&
						 (acct.Type == BAccountType.VendorType || acct.Type == BAccountType.CombinedType) &&
						 loc.LocationID != acct.DefLocationID)
				{
					Vendor vendor = PXSelect<Vendor>.Search<Vendor.bAccountID>(this, acct.BAccountID);
					VendorClass vClass = PXSelect<VendorClass>.Search<VendorClass.vendorClassID>(this, vendor.VendorClassID);
					if (vClass != null && vClass.TaxZoneID != null)
					{
						e.NewValue = vClass.TaxZoneID;
						e.Cancel = true;
					}
				}
			}
		}
		protected virtual void Location_VTaxCalcMode_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			Location loc = e.Row as Location;
			if (loc != null && loc.BAccountID != null)
			{
				BAccount acct = CR.Location.FK.BAccount.FindParent(this, loc);
				if (acct != null &&
						 acct.DefLocationID != null &&
						 (acct.Type == BAccountType.VendorType || acct.Type == BAccountType.CombinedType) &&
						 loc.LocationID != acct.DefLocationID)
				{
					Vendor vendor = PXSelect<Vendor>.Search<Vendor.bAccountID>(this, acct.BAccountID);
					VendorClass vClass = PXSelect<VendorClass>.Search<VendorClass.vendorClassID>(this, vendor.VendorClassID);
					if (vClass != null && vClass.TaxCalcMode != null)
					{
						e.NewValue = vClass.TaxCalcMode;
						e.Cancel = true;
					}
				}
			}

		}

		protected virtual void Location_CTaxZoneID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			Location loc = e.Row as Location;
			if (loc != null && loc.BAccountID != null)
			{
				BAccount acct = CR.Location.FK.BAccount.FindParent(this, loc);
				if (acct != null &&
						 acct.DefLocationID != null &&
						 (acct.Type == BAccountType.CustomerType || acct.Type == BAccountType.CombinedType) &&
						 loc.LocationID != acct.DefLocationID)
				{
					Customer customer = PXSelect<Customer>.Search<Customer.bAccountID>(this, acct.BAccountID);
					CustomerClass cClass = PXSelect<CustomerClass>.Search<CustomerClass.customerClassID>(this, customer.CustomerClassID);
					if (cClass != null && cClass.TaxZoneID != null)
					{
						e.NewValue = cClass.TaxZoneID;
						e.Cancel = true;
					}
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<Location.cTaxZoneID> e)
		{
			Location location = (Location)e.Row;
			if (location == null) return;
			if ((location.LocType == LocTypeList.VendorLoc || location.LocType == LocTypeList.CombinedLoc)
				&& (location.VTaxZoneID == null || (string)e.OldValue == location.VTaxZoneID))
			{
				Vendor vendor = PXSelect<Vendor>.Search<Vendor.bAccountID>(this, location.BAccountID);
				VendorClass vClass = PXSelect<VendorClass>.Search<VendorClass.vendorClassID>(this, vendor.VendorClassID);
				if (vClass?.RequireTaxZone ?? false)
				{
					this.LocationCurrent.Cache.SetValue<Location.vTaxZoneID>(location, location.CTaxZoneID);
				}
			}
		}
		protected virtual void _(Events.FieldUpdated<Location.vTaxZoneID> e)
		{
			Location location = (Location)e.Row;
			if (location == null) return;
			if ((location.LocType == LocTypeList.CustomerLoc || location.LocType == LocTypeList.CombinedLoc)
				&& (location.CTaxZoneID == null || (string)e.OldValue == location.CTaxZoneID))
			{
				Customer customer = PXSelect<Customer>.Search<Customer.bAccountID>(this, location.BAccountID);
				CustomerClass cClass = PXSelect<CustomerClass>.Search<CustomerClass.customerClassID>(this, customer.CustomerClassID);
				if (cClass?.RequireTaxZone ?? false)
				{
					this.LocationCurrent.Cache.SetValue<Location.cTaxZoneID>(location, location.VTaxZoneID);
				}
			}
		}

		protected virtual void Location_CAvalaraCustomerUsageType_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			Location loc = e.Row as Location;
			if (loc != null && loc.BAccountID != null)
			{
				BAccount acct = CR.Location.FK.BAccount.FindParent(this, loc);
				if (acct != null &&
						 acct.DefLocationID != null &&
						 (acct.Type == BAccountType.CustomerType || acct.Type == BAccountType.CombinedType) &&
						 loc.LocationID != acct.DefLocationID)
				{
					Customer customer = PXSelect<Customer>.Search<Customer.bAccountID>(this, acct.BAccountID);
					CustomerClass cClass = PXSelect<CustomerClass>.Search<CustomerClass.customerClassID>(this, customer.CustomerClassID);
					if (cClass != null && cClass.AvalaraCustomerUsageType != null)
					{
						e.NewValue = cClass.AvalaraCustomerUsageType;
						e.Cancel = true;
						return;
					}
				}

				e.NewValue = TXAvalaraCustomerUsageType.Default;
				e.Cancel = true;
			}
		}

		protected virtual void Location_CShipComplete_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			Location row = e.Row as Location;
			if (row != null)
			{
				BAccount acct = CR.Location.FK.BAccount.FindParent(this, row);
				if (acct != null)
				{
					CRLocation DefLocation = PXSelect<
							CRLocation,
						Where<
							CRLocation.bAccountID, Equal<Required<BAccount.bAccountID>>,
							And<CRLocation.locationID, Equal<Required<BAccount.defLocationID>>>>>
						.Select(this, acct.BAccountID, acct.DefLocationID);

					if (DefLocation != null && DefLocation.CShipComplete != null)
						e.NewValue = DefLocation.CShipComplete;
				}
			}
		}

		protected virtual void Location_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			Location loc = e.Row as Location;
			if (loc == null || loc.BAccountID == null) return;
			
			BAccount acct = CR.Location.FK.BAccount.FindParent(this, loc);
			if (acct != null)
			{
				var isDefault = cache.GetValueOriginal<CRLocation.isDefault>(loc) as bool?;
				if (loc.IsDefault == true && isDefault != loc.IsDefault)
				{
					SetDefaultLocation(loc, acct);
				}

				EstablishCTaxZoneRule((field, isRequired) => PXDefaultAttribute.SetPersistingCheck(cache, field.Name, loc, isRequired ? PXPersistingCheck.Null : PXPersistingCheck.Nothing));
				EstablishVTaxZoneRule((field, isRequired) => PXDefaultAttribute.SetPersistingCheck(cache, field.Name, loc, isRequired ? PXPersistingCheck.Null : PXPersistingCheck.Nothing));

				if (acct.Type == BAccountType.CustomerType || acct.Type == BAccountType.CombinedType)
				{
					LocationValidator.ValidateCustomerLocation(cache, acct, loc);
					VerifyAvalaraUsageType(loc);
				}
				if (acct.Type == BAccountType.VendorType || acct.Type == BAccountType.CombinedType)
				{
					var vendor = VendorMaint.GetByID(this, acct.BAccountID);
					LocationValidator.ValidateVendorLocation(cache, vendor, loc);
				}
			}
		}

		object _KeyToAbort = null;

		protected virtual void Location_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.Operation == PXDBOperation.Insert)
			{
				if (e.TranStatus == PXTranStatus.Open)
				{
					if ((int?)sender.GetValue<CR.Location.vAPAccountLocationID>(e.Row) < 0)
					{
						_KeyToAbort = sender.GetValue<Location.locationID>(e.Row);

						PXDatabase.Update<Location>(
							new PXDataFieldAssign("VAPAccountLocationID", _KeyToAbort),
							new PXDataFieldRestrict("LocationID", _KeyToAbort),
							PXDataFieldRestrict.OperationSwitchAllowed);

						sender.SetValue<CR.Location.vAPAccountLocationID>(e.Row, _KeyToAbort);
					}

					if ((int?)sender.GetValue<CR.Location.vPaymentInfoLocationID>(e.Row) < 0)
					{
						_KeyToAbort = sender.GetValue<Location.locationID>(e.Row);

						// Acuminator disable once PX1043 SavingChangesInEventHandlers [Legacy]
						PXDatabase.Update<Location>(
							new PXDataFieldAssign("VPaymentInfoLocationID", _KeyToAbort),
							new PXDataFieldRestrict("LocationID", _KeyToAbort),
							PXDataFieldRestrict.OperationSwitchAllowed);

						sender.SetValue<CR.Location.vPaymentInfoLocationID>(e.Row, _KeyToAbort);
					}

					if ((int?)sender.GetValue<CR.Location.cARAccountLocationID>(e.Row) < 0)
					{
						_KeyToAbort = sender.GetValue<Location.locationID>(e.Row);

						// Acuminator disable once PX1043 SavingChangesInEventHandlers [Legacy]
						PXDatabase.Update<Location>(
							new PXDataFieldAssign("CARAccountLocationID", _KeyToAbort),
							new PXDataFieldRestrict("LocationID", _KeyToAbort),
							PXDataFieldRestrict.OperationSwitchAllowed);

						sender.SetValue<CR.Location.cARAccountLocationID>(e.Row, _KeyToAbort);
					}
				}
				else
				{
					if (e.TranStatus == PXTranStatus.Aborted)
					{
						if (object.Equals(_KeyToAbort, sender.GetValue<CR.Location.vAPAccountLocationID>(e.Row)))
						{
							object KeyAborted = sender.GetValue<Location.locationID>(e.Row);
							sender.SetValue<CR.Location.vAPAccountLocationID>(e.Row, KeyAborted);
						}

						if (object.Equals(_KeyToAbort, sender.GetValue<CR.Location.vPaymentInfoLocationID>(e.Row)))
						{
							object KeyAborted = sender.GetValue<Location.locationID>(e.Row);
							sender.SetValue<CR.Location.vPaymentInfoLocationID>(e.Row, KeyAborted);
						}

						if (object.Equals(_KeyToAbort, sender.GetValue<CR.Location.cARAccountLocationID>(e.Row)))
						{
							object KeyAborted = sender.GetValue<Location.locationID>(e.Row);
							sender.SetValue<CR.Location.cARAccountLocationID>(e.Row, KeyAborted);
						}
					}
					_KeyToAbort = null;
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<Location, Location.status> e)
		{
			if (e.Row == null) return;
			switch (e.Row.Status)
			{
				case LocationStatus.Active:
					e.Cache.SetValue<Location.isActive>(e.Row, true);
					break;
				default:
					e.Cache.SetValue<Location.isActive>(e.Row, false);
					break;
			}
		}

		protected virtual void Location_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			Location row = e.Row as Location;
			if (row != null)
			{
				Address addr = PXSelect<Address, Where<Address.addressID, Equal<Required<Address.addressID>>>>.Select(this, row.DefAddressID);
				if (addr != null && Address.Cache.GetStatus(addr) == PXEntryStatus.Notchanged)
				{
					//LocationExtAddress projection is sensitive to Address.tstamp only
					Address.Cache.SetStatus(addr, PXEntryStatus.Updated);
				}
			}
		}

		protected virtual void Location_IsARAccountSameAsMain_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CR.Location record = (CR.Location)e.Row;
			BAccount baccount = CR.Location.FK.BAccount.FindParent(this, record);

			if (record.IsARAccountSameAsMain == false)
			{
				LocationARAccountSub mainloc = ARAccountSubLocation.Select();
				record.CARAccountID = mainloc.CARAccountID;
				record.CARSubID = mainloc.CARSubID;
				record.CRetainageAcctID = mainloc.CRetainageAcctID;
				record.CRetainageSubID = mainloc.CRetainageSubID;
				record.CARAccountLocationID = record.LocationID;

				LocationARAccountSub copyloc = new LocationARAccountSub();
				copyloc.BAccountID = record.BAccountID;
				copyloc.LocationID = record.LocationID;
				copyloc.CARAccountID = record.CARAccountID;
				copyloc.CARSubID = record.CARSubID;
				copyloc.CRetainageAcctID = record.CRetainageAcctID;
				copyloc.CRetainageSubID = record.CRetainageSubID;

				BusinessAccount.Cache.Current = baccount;
				ARAccountSubLocation.Insert(copyloc);
			}
			if (record.IsARAccountSameAsMain == true && baccount.DefLocationID != record.LocationID)
			{
				record.CARAccountID = null;
				record.CARSubID = null;
				record.CRetainageAcctID = null;
				record.CRetainageSubID = null;
				
				if (baccount != null)
				{
					record.CARAccountLocationID = baccount.DefLocationID;
				}
			}
		}

		protected virtual void Location_VBranchID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = null;
			e.Cancel = true;
		}

		#endregion

		#region Address Events

		protected virtual void Address_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			Address row = e.Row as Address;
			if (row == null) return;

			PXUIFieldAttribute.SetEnabled<Address.isValidated>(cache, row, false);
		}

		#endregion

		#region LocationARAccountSub Events

		protected virtual void LocationARAccountSub_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (Location.Current != null)
			{
				PXUIFieldAttribute.SetEnabled(sender, e.Row, object.Equals(Location.Current.LocationID, Location.Current.CARAccountLocationID));
			}
		}

		protected virtual void LocationARAccountSub_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			LocationARAccountSub record = (LocationARAccountSub)e.Row;

			if (!sender.ObjectsEqual<LocationARAccountSub.cARAccountID, LocationARAccountSub.cARSubID>(e.Row, e.OldRow))
			{
				Location mainloc = Location.Current;
				var state = (PXFieldState)sender.GetStateExt<LocationARAccountSub.cARAccountID>(e.Row);
				if (state.ErrorLevel < PXErrorLevel.Error)
					mainloc.CARAccountID = record.CARAccountID;
				mainloc.CARSubID = record.CARSubID;

				Location.Cache.MarkUpdated(mainloc);

				Address addr = PXSelect<Address, Where<Address.addressID, Equal<Required<Address.addressID>>>>.Select(this, mainloc.DefAddressID);
				if (addr != null && Address.Cache.GetStatus(addr) == PXEntryStatus.Notchanged)
				{
					//LocationExtAddress projection is sensitive to Address.tstamp only
					Address.Cache.SetStatus(addr, PXEntryStatus.Updated);
				}
			}

			if (!sender.ObjectsEqual<LocationARAccountSub.cRetainageAcctID, LocationARAccountSub.cRetainageSubID>(e.Row, e.OldRow))
			{
				Location mainloc = Location.Current;
				var state = (PXFieldState)sender.GetStateExt<LocationARAccountSub.cRetainageAcctID>(e.Row);
				if (state.ErrorLevel < PXErrorLevel.Error)
					mainloc.CRetainageAcctID = record.CRetainageAcctID;
				mainloc.CRetainageSubID = record.CRetainageSubID;

				if (Location.Cache.GetStatus(mainloc) == PXEntryStatus.Notchanged)
				{
					Location.Cache.SetStatus(mainloc, PXEntryStatus.Updated);
				}
			}
		}

		#endregion

		#region Extensions

		#region Address Lookup Extension
		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class LocationMaintAddressLookupExtension : CR.Extensions.AddressLookupExtension<LocationMaint, Location, Address>
		{
			protected override string AddressView => nameof(Base.Address);
			protected override string ViewOnMap => nameof(Base.viewOnMap);
		}
		#endregion

		#endregion

		#region Methods

		public override void Persist()
		{
			var arAccountSub = ARAccountSubLocation.Current;

			if (arAccountSub != null)
			{
				ValidationHelper.SetErrorEmptyIfNull<LocationARAccountSub.cARAccountID>(ARAccountSubLocation.Cache, arAccountSub, arAccountSub.CARAccountID);
				ValidationHelper.SetErrorEmptyIfNull<LocationARAccountSub.cARSubID>(ARAccountSubLocation.Cache, arAccountSub, arAccountSub.CARSubID);
			}

			base.Persist();
			this.ARAccountSubLocation.Cache.Clear();
		}

		private CustomerClass ReadCustomerClass()
		{
			CustomerClass customerClass = 
				(PXResult<AR.Customer, AR.CustomerClass>)PXSelectJoin<AR.Customer,
					InnerJoin<AR.CustomerClass, On<AR.Customer.customerClassID, Equal<AR.CustomerClass.customerClassID>>>,
					Where<AR.Customer.bAccountID, Equal<Current<Location.bAccountID>>>>.Select(this);

			return customerClass;
		}

		private VendorClass ReadVendorClass()
		{
			VendorClass vendorClass =
				(PXResult<AP.Vendor, AP.VendorClass>)PXSelectJoin<AP.Vendor,
					InnerJoin<AP.VendorClass, On<AP.Vendor.vendorClassID, Equal<AP.VendorClass.vendorClassID>>>,
					Where<AP.Vendor.bAccountID, Equal<Current<Location.bAccountID>>>>.Select(this);

			return vendorClass;
		}

		protected virtual void VerifyAvalaraUsageType(Location loc)
		{
			CustomerClass customerClass = ReadCustomerClass();
			if (customerClass == null || customerClass.RequireAvalaraCustomerUsageType != true)
				return;

			if (loc.CAvalaraCustomerUsageType == TXAvalaraCustomerUsageType.Default)
				throw new PXRowPersistingException(typeof(Location.cAvalaraCustomerUsageType).Name,
					loc.CAvalaraCustomerUsageType, Common.Messages.NonDefaultAvalaraUsageType);
		}

		protected virtual void EstablishCTaxZoneRule(Action<Type, bool> setCheck)
		{
			CustomerClass customerClass = ReadCustomerClass();
			VendorClass vendorClass = ReadVendorClass();

			bool isTaxZoneRequired = (customerClass?.RequireTaxZone ?? false) || (vendorClass?.RequireTaxZone ?? false);
			setCheck(typeof(Location.cTaxZoneID), isTaxZoneRequired);
		}

		protected virtual void EstablishVTaxZoneRule(Action<Type, bool> setCheck)
		{
			VendorClass vendorClass = ReadVendorClass();

			bool isTaxZoneRequred = vendorClass != null && vendorClass.RequireTaxZone == true;
			setCheck(typeof(Location.vTaxZoneID), isTaxZoneRequred);
		}

		protected virtual void SetDefaultLocation(Location row, BAccount account)
		{
			if (account == null || account.BAccountID == null) return;
			if (row == null || row.LocationID == null) return;

			if (row.IsActive != true)
				throw new Exception(Messages.DefaultLocationCanNotBeNotActive);

			var previuosDefault =
				(Location)PXSelect<
						Location,
					Where<
						Location.bAccountID, Equal<Required<BAccount.bAccountID>>,
						And<Location.locationID, Equal<Required<BAccount.defLocationID>>>>>
					.SelectSingleBound(this, null, account.BAccountID, account.DefLocationID);

			if (account.DefLocationID != row.LocationID)
				SetLocationAsDefault(row, previuosDefault);

			account.DefLocationID = row.LocationID;

			BusinessAccount.Cache.Update(account);
		}

		public virtual void SetLocationAsDefault(Location newDefault, Location previuosDefault)
		{
			int? defLocationID = newDefault.LocationID;

			foreach (Location other in this.OtherLocations.Select())
			{
				if (!object.Equals(other.LocationID, other.VAPAccountLocationID))
				{
					other.VAPAccountLocationID = defLocationID;
					if (object.Equals(other.LocationID, newDefault.LocationID) && previuosDefault != null)
					{
						other.VAPAccountID = previuosDefault.VAPAccountID;
						other.VAPSubID = previuosDefault.VAPSubID;
						other.VRetainageAcctID = previuosDefault.VRetainageAcctID;
						other.VRetainageSubID = previuosDefault.VRetainageSubID;
					}

					Location.Cache.MarkUpdated(other);
				}

				if (!object.Equals(other.LocationID, other.VPaymentInfoLocationID))
				{
					other.VPaymentInfoLocationID = defLocationID;
					if (object.Equals(other.LocationID, newDefault.LocationID) && previuosDefault != null)
					{
						other.VCashAccountID = previuosDefault.VCashAccountID;
						other.VPaymentMethodID = previuosDefault.VPaymentMethodID;
						other.VPaymentLeadTime = previuosDefault.VPaymentLeadTime;
						other.VSeparateCheck = previuosDefault.VSeparateCheck;
						other.VPaymentByType = previuosDefault.VPaymentByType;
						other.VRemitAddressID = previuosDefault.VRemitAddressID;
						other.VRemitContactID = previuosDefault.VRemitContactID;
					}

					Location.Cache.MarkUpdated(other);
				}

				if (!object.Equals(other.LocationID, other.CARAccountLocationID))
				{
					other.CARAccountLocationID = defLocationID;
					if (object.Equals(other.LocationID, newDefault.LocationID) && previuosDefault != null)
					{
						other.CARAccountID = previuosDefault.CARAccountID;
						other.CARSubID = previuosDefault.CARSubID;
						other.CRetainageAcctID = previuosDefault.CRetainageAcctID;
						other.CRetainageSubID = previuosDefault.CRetainageSubID;
					}

					Location.Cache.MarkUpdated(other);
				}
			}
		}

		private Location GetDefaultLocation(PXCache cache, Location location)
		{
			if (location != null && location.BAccountID != null)
			{
				BAccount acct = (BAccount)PXParentAttribute.SelectParent(cache, location, typeof(BAccount));
				if (acct != null &&
						 acct.DefLocationID != null &&
						 (acct.Type == BAccountType.VendorType || acct.Type == BAccountType.CustomerType ||acct.Type == BAccountType.CombinedType) &&
						 location.LocationID != acct.DefLocationID)
				{
					return PXSelect<Location>.Search<Location.locationID, Location.bAccountID>(this, acct.DefLocationID, acct.BAccountID);
				}
			}
			return null;
		}

		#endregion
	}

	public class LocationValidator
	{
		public virtual bool ValidateCustomerLocation(PXCache cache, BAccount baccount, ILocation location)
		{
			bool res = ValidateCommon(cache, baccount, location);

			var validationHelper = new AccountAndSubValidationHelper(cache, location);

			res &= validationHelper.SetErrorEmptyIfNull<Location.cSalesAcctID>(location.CSalesAcctID)
				.SetErrorEmptyIfNull<Location.cSalesSubID>(location.CSalesSubID)
				.SetErrorIfInactiveAccount<Location.cARAccountID>(location.CARAccountID)
				.SetErrorIfInactiveSubAccount<Location.cARSubID>(location.CARSubID)
				.SetErrorIfInactiveAccount<Location.cSalesAcctID>(location.CSalesAcctID)
				.SetErrorIfInactiveSubAccount<Location.cSalesSubID>(location.CSalesSubID)
				.SetErrorIfInactiveAccount<Location.cDiscountAcctID>(location.CDiscountAcctID)
				.SetErrorIfInactiveSubAccount<Location.cDiscountSubID>(location.CDiscountSubID)
				.SetErrorIfInactiveAccount<Location.cFreightAcctID>(location.CFreightAcctID)
				.SetErrorIfInactiveSubAccount<Location.cFreightSubID>(location.CFreightSubID)
				.IsValid;

			if (location.IsARAccountSameAsMain != true)
			{
				res &= validationHelper.SetErrorEmptyIfNull<Location.cARAccountID>(location.CARAccountID)
					.SetErrorEmptyIfNull<Location.cARSubID>(location.CARSubID)
					.IsValid;
			}

			return res;
		}

		public virtual bool ValidateVendorLocation(PXCache cache, Vendor vendor, ILocation location)
		{
			bool res = ValidateCommon(cache, vendor, location);

			var validationHelper = new ValidationHelper(cache, location);

			if (vendor.TaxAgency == true)
			{
				res &= validationHelper.SetErrorEmptyIfNull<Location.vExpenseAcctID>(location.VExpenseAcctID)
					.SetErrorEmptyIfNull<Location.vExpenseSubID>(location.VExpenseSubID)
					.IsValid;
			}

			if (location.IsAPAccountSameAsMain != true)
			{
				res &= validationHelper.SetErrorEmptyIfNull<Location.vAPAccountID>(location.VAPAccountID)
					.SetErrorEmptyIfNull<Location.vAPSubID>(location.VAPSubID)
					.IsValid;
			}

			return res;
		}

		protected virtual bool ValidateCommon(PXCache cache, BAccount acct, ILocation loc)
		{
			if (loc.LocationID == acct.DefLocationID && loc.IsActive != true)
			{
				cache.RaiseExceptionHandling<Location.isActive>(loc, null, new PXSetPropertyException(Messages.DefaultLocationCanNotBeNotActive, $"[{nameof(Location.isActive)}]"));
				return false;
			}

			return true;
		}
	}
}
