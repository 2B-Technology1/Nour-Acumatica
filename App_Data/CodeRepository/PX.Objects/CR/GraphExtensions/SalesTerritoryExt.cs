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
using PX.Objects.CS;

namespace PX.Objects.CR.Extensions
{
	/// <exclude/>
	public abstract class
		SalesTerritoryExt<
			TGraph,
			TMaster,
			FMasterAddressID,
			FOverrideSalesTerritory,
			FSalesTerritoryID,
			TAddress,
			FAddressAddressID,
			FAddressCountryID,
			FAddressState>
		: PXGraphExtension<TGraph>
			where TGraph : PXGraph
			where TMaster : class, IBqlTable, new()
			where FMasterAddressID : class, IBqlField
			where FOverrideSalesTerritory : class, IBqlField
			where FSalesTerritoryID : class, IBqlField
			where TAddress : class, IAddressBase, IBqlTable, new()
			where FAddressAddressID : class, IBqlField
			where FAddressCountryID : class, IBqlField
			where FAddressState : class, IBqlField
	{
		protected static bool IsExtensionActive() => PXAccess.FeatureInstalled<FeaturesSet.salesTerritoryManagement>();

		protected abstract IAddressBase CurrentAddress { get; }

		#region Events
		protected virtual void _(Events.RowInserted<TMaster> e)
		{
			if (e.Row == null) return;

			AssignDefaultSalesTerritory(this.CurrentAddress);
		}

		protected virtual void _(Events.FieldUpdated<FMasterAddressID> e)
		{
			if (e.Row == null) return;

			AssignDefaultSalesTerritory(this.CurrentAddress);
		}

		protected virtual void _(Events.RowInserted<TAddress> e)
		{
			if (e.Row == null) return;

			AssignDefaultSalesTerritory(e.Row);
		}

		protected virtual void _(Events.RowUpdated<TAddress> e)
		{
			var realAddress = this.CurrentAddress;

			if (e.Row == null
				|| !e.Cache.ObjectsEqual<FAddressAddressID>(e.Row, realAddress)
				|| e.Cache.ObjectsEqual<FAddressCountryID, FAddressState>(e.Row, e.OldRow))
				return;

			AssignDefaultSalesTerritory(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<TMaster, FOverrideSalesTerritory> e)
		{
			if (e.Row == null) return;

			if (e.NewValue is false)
			{
				e.Cache.SetValue<FSalesTerritoryID>(e.Row, GetSalesTerritory(this.CurrentAddress));
			}
		}

		#endregion

		#region Methods

		protected virtual void UpdateRelatedContacts(int? mainContactID, int? addressID, string salesTerritoryID)
		{
			foreach (Contact contact in SelectFrom<Contact>
										.Where<Contact.defAddressID.IsEqual<@P.AsInt>
											.And<Contact.contactID.IsNotEqual<@P.AsInt>>
											.And<Contact.overrideSalesTerritory.IsEqual<False>
											.And<
												Where<
													Contact.salesTerritoryID.IsNull
													.Or<Contact.salesTerritoryID.IsNotEqual<@P.AsString>>>>>>
									.View
									.Select(Base, new object[] { addressID, mainContactID, salesTerritoryID }))
			{
				contact.SalesTerritoryID = salesTerritoryID;
				Base.Caches[typeof(Contact)].Update(contact);
			}
		}

		protected virtual void UpdateRelatedBAccount(int? addressID, string salesTerritoryID)
		{
			foreach (BAccount account in SelectFrom<BAccount>
										.Where<BAccount.defAddressID.IsEqual<@P.AsInt>
											.And<BAccount.overrideSalesTerritory.IsEqual<False>
											.And<
												Where<
													BAccount.salesTerritoryID.IsNull
													.Or<BAccount.salesTerritoryID.IsNotEqual<@P.AsString>>>>>>
									.View
									.Select(Base, new object[] { addressID, salesTerritoryID }))
			{
				account.SalesTerritoryID = salesTerritoryID;
				Base.Caches[typeof(BAccount)].Update(account);
			}
		}

		protected virtual void AssignDefaultSalesTerritory(IAddressBase address)
		{
			var masterRow = Base.GetCurrentPrimaryObject();

			if (masterRow == null) return;

			var cache = Base.GetPrimaryCache();

			if (cache.GetValue<FOverrideSalesTerritory>(masterRow) is true)
			{
				return;
			}
			cache.SetValue<FSalesTerritoryID>(masterRow, GetSalesTerritory(address));
		}

		public virtual string GetSalesTerritory(IAddressBase address)
		{
			SalesTerritory territory = null;

			if (address == null)
				return null;

			if (address?.CountryID != null && address?.State != null)
			{
				territory = SelectFrom<SalesTerritory>
				.InnerJoin<State>
					.On<State.salesTerritoryID.IsEqual<SalesTerritory.salesTerritoryID>>
				.Where<
					SalesTerritory.salesTerritoryType.IsEqual<SalesTerritoryTypeAttribute.byState>
					.And<SalesTerritory.isActive.IsEqual<True>>
					.And<State.countryID.IsEqual<@P.AsString>>
					.And<State.stateID.IsEqual<@P.AsString>>
					>
				.View
				.SelectSingleBound(Base,
					currents: null,
					new object[] { address.CountryID, address.State });
			}

			if (address?.CountryID != null && territory == null)
			{
				territory = SelectFrom<SalesTerritory>
				.InnerJoin<Country>
					.On<Country.salesTerritoryID.IsEqual<SalesTerritory.salesTerritoryID>>
				.Where<
					SalesTerritory.salesTerritoryType.IsEqual<SalesTerritoryTypeAttribute.byCountry>
					.And<SalesTerritory.isActive.IsEqual<True>>
					.And<Country.countryID.IsEqual<@P.AsString>>
					>
				.View
				.SelectSingleBound(Base,
					currents: null,
					new object[] { address.CountryID });
			}

			return territory?.SalesTerritoryID;
		}

		#endregion
	}
}
