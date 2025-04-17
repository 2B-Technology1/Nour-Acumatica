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

namespace PX.Objects.CR.BusinessAccountMaint_Extensions
{
	/// <exclude/>
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class BusinessAccountMaint_SalesTerritoryExt :
			SalesTerritoryExt<
					BusinessAccountMaint,
					BAccount,
					BAccount.defAddressID,
					BAccount.overrideSalesTerritory,
					BAccount.salesTerritoryID,
					Address,
					Address.addressID,
					Address.countryID,
					Address.state>
	{
		public static bool IsActive() => IsExtensionActive();

		protected override IAddressBase CurrentAddress => Base.GetExtension<BusinessAccountMaint.DefContactAddressExt>()?.DefAddress?.SelectSingle();

		protected override void AssignDefaultSalesTerritory(IAddressBase address)
		{
			base.AssignDefaultSalesTerritory(address);

			if (address is Address addr && addr.AddressID > 0)
			{
				var account = Base.CurrentBAccount.SelectSingle();

				if (account == null || addr.AddressID != account.DefAddressID)
					return;

				var salesTerritoryID = GetSalesTerritory(address);
				UpdateRelatedContacts(account.DefContactID, addr.AddressID, salesTerritoryID);

				var accuntExt = Base.GetExtension<BusinessAccountMaint.UpdateRelatedContactInfoFromAccountGraphExt>();

				foreach (Address relAddr in
							accuntExt.BAccountRelatedAddresses
							.Select(account.BAccountID))
				{
					UpdateRelatedContacts(account.DefContactID, relAddr.AddressID, salesTerritoryID);
				}
			}
		}
	}
}
