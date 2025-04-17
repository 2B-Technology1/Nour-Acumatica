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
using static PX.Objects.CR.BackwardCompatibility.CRLeadBackwardCompatibility;
using static PX.Objects.CR.ContactTypesAttribute;

namespace PX.Objects.CR.LeadMaint_Extensions
{
	/// <exclude/>
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class LeadMaint_SalesTerritoryExt :
			SalesTerritoryExt<
					LeadMaint,
					CRLead,
					CRLead.defAddressID,
					CRLead.overrideSalesTerritory,
					CRLead.salesTerritoryID,
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

			var lead = Base.LeadCurrent.SelectSingle();
			var salesTerritoryID = GetSalesTerritory(address);

			if (address is Address addr && addr.AddressID > 0)
			{
				UpdateRelatedContacts(lead.ContactID, addr.AddressID, salesTerritoryID);
				UpdateRelatedBAccount(addr.AddressID, salesTerritoryID);
			}

			if (lead?.RefContactID != null)
			{
				var leadExt = Base.GetExtension<LeadMaint.UpdateRelatedContactInfoFromLeadGraphExt>();

				foreach (Address relAddr in
							leadExt.RefContactRelatedAddresses
							.Select(lead.RefContactID, lead.RefContactID, lead.RefContactID))
				{
					UpdateRelatedContacts(lead.ContactID, relAddr.AddressID, salesTerritoryID);
					UpdateRelatedBAccount(relAddr.AddressID, salesTerritoryID);
				}
			}
			else if (lead?.BAccountID != null)
			{
				var leadExt = Base.GetExtension<LeadMaint.UpdateRelatedContactInfoFromLeadGraphExt>();

				foreach (Address relAddr in
							leadExt.BAccountRelatedAddresses
							.Select(lead.BAccountID, lead.BAccountID))
				{
					UpdateRelatedContacts(lead.ContactID, relAddr.AddressID, salesTerritoryID);
					UpdateRelatedBAccount(relAddr.AddressID, salesTerritoryID);
				}
			}
		}
	}
}
