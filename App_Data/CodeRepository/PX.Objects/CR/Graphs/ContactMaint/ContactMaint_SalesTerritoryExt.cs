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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CR.Extensions;

namespace PX.Objects.CR.ContactMaint_Extensions
{
	/// <exclude/>
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class ContactMaint_SalesTerritoryExt :
			SalesTerritoryExt<
					ContactMaint,
					Contact,
					Contact.defAddressID,
					Contact.overrideSalesTerritory,
					Contact.salesTerritoryID,
					Address,
					Address.addressID,
					Address.countryID,
					Address.state>
	{
		public static bool IsActive() => IsExtensionActive();

		protected override IAddressBase CurrentAddress => Base.AddressCurrent.SelectSingle();

		protected override void AssignDefaultSalesTerritory(IAddressBase address)
		{
			base.AssignDefaultSalesTerritory(address);

			if (address is Address addr && addr.AddressID > 0)
			{
				Contact contact = Base.Contact.Current ?? PXSelect<
							Contact,
						Where<
							Contact.defAddressID, Equal<@P.AsInt>,
							And<Contact.contactType, Equal<ContactTypesAttribute.person>>>>
					.Select(Base, addr.AddressID);

				if (contact == null)
					return;

				var salesTerritoryID = GetSalesTerritory(address);
				UpdateRelatedContacts(contact.ContactID, addr.AddressID, salesTerritoryID);
				UpdateRelatedBAccount(addr.AddressID, salesTerritoryID);

				var contactExt = Base.GetExtension<ContactMaint.UpdateRelatedContactInfoFromContactGraphExt>();

				foreach (Address relAddr in
							contactExt.ContactRelatedAddresses
							.Select(contact.ContactID))
				{
					UpdateRelatedContacts(contact.ContactID, relAddr.AddressID, salesTerritoryID);
					UpdateRelatedBAccount(relAddr.AddressID, salesTerritoryID);
				}
			}
		}
	}
}
