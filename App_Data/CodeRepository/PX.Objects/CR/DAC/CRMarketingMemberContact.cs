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
using PX.Objects.CS;
using PX.SM;

namespace PX.Objects.CR
{
	/// <exclude/>
	[Serializable]
	[PXHidden]
	[PXProjection(typeof(Select2<Contact,
		InnerJoin<Address, On<True, Equal<False>>,
		InnerJoin<Standalone.CRLead, On<True, Equal<False>>>>>
		), Persistent = false)]
	[PXVirtual]
	[PXBreakInheritance]
	public class CRMarketingMemberForImport : Contact
	{
		#region New Fields
	
		#region MarketingListID
		public abstract class linkMarketingListID : PX.Data.BQL.BqlInt.Field<linkMarketingListID> { }

		[PXDBInt]
		[PXUIField(DisplayName = "Marketing List ID")]
		[PXSelector(typeof(Search<CRMarketingList.marketingListID, Where<CRMarketingList.type, Equal<CRMarketingList.type.@static>>>), DescriptionField = typeof(CRMarketingList.mailListCode))]
		public virtual Int32? LinkMarketingListID { get; set; }
		#endregion

		#region CampaignID
		public abstract class linkCampaignID : PX.Data.BQL.BqlString.Field<linkCampaignID> { }

		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Campaign ID")]
		[PXSelector(typeof(CRCampaign.campaignID), DescriptionField = typeof(CRCampaign.campaignName))]
		public virtual String LinkCampaignID { get; set; }
		#endregion

		#endregion

		#region Contact Overrides

		#region ContactID
		public new abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }

		[PXDBIdentity(IsKey = true, BqlField = typeof(contactID))]
		[PXUIField(DisplayName = "Member Name", Visibility = PXUIVisibility.Invisible)]
		[ContactSelector(true, typeof(ContactTypesAttribute.person), typeof(ContactTypesAttribute.employee))]
		public override Int32? ContactID { get; set; }
		#endregion

		#region ExistingContactID
		public virtual Int32? ExistingContactID { get; set; }
		#endregion

		#region ContactType
		public new abstract class contactType : PX.Data.BQL.BqlString.Field<contactType> { }

		[PXDBString(2, IsFixed = true)]
		[PXDefault(ContactTypesAttribute.Person)]
		[ContactTypes]
		[PXUIField(DisplayName = "Type")]
		public override String ContactType { get; set; }
		#endregion

		#endregion

		#region Address

		#region AddressType
		public abstract class addressType : PX.Data.BQL.BqlString.Field<addressType> { }

		[PXDBString(2, IsFixed = true, BqlField = typeof(Address.addressType))]
		[PXDefault(CR.Address.AddressTypes.BusinessAddress)]
		[CR.Address.AddressTypes.List()]
		[PXUIField(DisplayName = "AddressType", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String AddressType { get; set; }
		#endregion

		#region AddressLine1
		public abstract class addressLine1 : PX.Data.BQL.BqlString.Field<addressLine1> { }

		[PXDBString(50, IsUnicode = true, BqlField = typeof(Address.addressLine1))]
		[PXUIField(DisplayName = "Address Line 1", Visibility = PXUIVisibility.SelectorVisible)]
		[PXPersonalDataField]
		public virtual String AddressLine1 { get; set; }
		#endregion

		#region AddressLine2
		public abstract class addressLine2 : PX.Data.BQL.BqlString.Field<addressLine2> { }

		[PXDBString(50, IsUnicode = true, BqlField = typeof(Address.addressLine2))]
		[PXUIField(DisplayName = "Address Line 2")]
		[PXPersonalDataField]
		public virtual String AddressLine2 { get; set; }
		#endregion

		#region AddressLine3
		public abstract class addressLine3 : PX.Data.BQL.BqlString.Field<addressLine3> { }

		[PXDBString(50, IsUnicode = true, BqlField = typeof(Address.addressLine3))]
		[PXUIField(DisplayName = "Address Line 3")]
		[PXPersonalDataField]
		public virtual String AddressLine3 { get; set; }
		#endregion

		#region City
		public abstract class city : PX.Data.BQL.BqlString.Field<city> { }

		[PXDBString(50, IsUnicode = true, BqlField = typeof(Address.city))]
		[PXUIField(DisplayName = "City", Visibility = PXUIVisibility.SelectorVisible)]
		[PXPersonalDataField]
		public virtual String City { get; set; }
		#endregion

		#region CountryID
		public abstract class countryID : PX.Data.BQL.BqlString.Field<countryID> { }

		[PXDefault(typeof(Search<GL.Branch.countryID, Where<GL.Branch.branchID, Equal<Current<AccessInfo.branchID>>>>))]
		[PXDBString(100, BqlField = typeof(Address.countryID))]
		[PXUIField(DisplayName = "Country")]
		[Country]
		public virtual String CountryID { get; set; }
		#endregion

		#region State
		public abstract class state : PX.Data.BQL.BqlString.Field<state> { }

		[PXDBString(50, IsUnicode = true, BqlField = typeof(Address.state))]
		[PXUIField(DisplayName = "State")]
		[State(typeof(countryID))]
		public virtual String State { get; set; }
		#endregion

		#region PostalCode
		public abstract class postalCode : PX.Data.BQL.BqlString.Field<postalCode> { }

		[PXDBString(20, BqlField = typeof(Address.postalCode))]
		[PXUIField(DisplayName = "Postal Code")]
		[PXZipValidation(typeof(Country.zipCodeRegexp), typeof(Country.zipCodeMask), countryIdField: typeof(countryID))]
		[PXPersonalDataField]
		public virtual String PostalCode { get; set; }
		#endregion

		#endregion

		#region CRLead

		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }

		[PXDBString(255, IsUnicode = true, BqlField = typeof(Standalone.CRLead.description))]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Description { get; set; }
		#endregion

		#endregion
	}
}
