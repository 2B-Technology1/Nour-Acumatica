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
using System.Collections;
using PX.Data;
using PX.Objects.Common;
using PX.Objects.CR.Extensions.Relational;
using PX.Objects.CS;

namespace PX.Objects.CR.Extensions
{
	/// <summary>
	/// Extension that is used for selecting and defaulting the Default Address and Default Contact of the Business Account and it's inheritors.
	/// No Inserting of Contact and Address is implemented, as the Inserting is handled inside the <see cref="SharedChildOverrideGraphExt{TGraph,TThis}"/> graph extension.
	/// </summary>
	public abstract class DefContactAddressExt<TGraph, TMaster, FAcctName> : PXGraphExtension<TGraph>, IAddressValidationHelper
		where TGraph : PXGraph
		where TMaster : BAccount, IBqlTable, new()
		where FAcctName : class, IBqlField
	{
		#region State

		protected virtual bool PersistentAddressValidation => false;

		#endregion

		#region Views

		[PXViewName(Messages.AccountAddress)]
		public PXSelect<
				Address,
			Where<
				Address.bAccountID, Equal<Current<BAccount.bAccountID>>,
				And<Address.addressID, Equal<Current<BAccount.defAddressID>>>>>
			DefAddress;

		[PXViewName(Messages.AccountContact)]
		[PXCopyPasteHiddenFields(typeof(Contact.duplicateStatus), typeof(Contact.duplicateFound))]
		public PXSelect<
				Contact,
			Where<
				Contact.bAccountID, Equal<Current<BAccount.bAccountID>>,
				And<Contact.contactID, Equal<Current<BAccount.defContactID>>>>>
			DefContact;

		#endregion

		#region ctor

		public override void Initialize()
		{
			base.Initialize();

			PXUIFieldAttribute.SetEnabled<Contact.fullName>(this.DefContact.Cache, null);

			PXUIFieldAttribute.SetVisible<Contact.languageID>(this.DefContact.Cache, null, PXDBLocalizableStringAttribute.HasMultipleLocales);
		}

		#endregion

		#region Actions

		public PXAction<TMaster> ViewMainOnMap;
		[PXUIField(DisplayName = Messages.ViewOnMap, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable viewMainOnMap(PXAdapter adapter)
		{
			if (this.DefAddress.SelectSingle() is Address addr)
			{
				BAccountUtility.ViewOnMap(addr);
			}

			return adapter.Get();
		}

		public PXAction<TMaster> ValidateAddresses;
		[PXUIField(DisplayName = CS.Messages.ValidateAddresses, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, FieldClass = CS.Messages.ValidateAddress)]
		[PXButton]
		public virtual IEnumerable validateAddresses(PXAdapter adapter)
		{
			var account = Base.Caches[typeof(TMaster)].Current as TMaster;

			if (account == null)
				return adapter.Get();

			if (PersistentAddressValidation && Base.IsDirty)
				Base.Actions.PressSave();

			Base.FindAllImplementations<IAddressValidationHelper>().ValidateAddresses();

			if (PersistentAddressValidation)
				Base.Actions.PressSave();

			return adapter.Get();
		}

		#endregion

		#region Methods

		public virtual bool CurrentAddressRequiresValidation => true;

		public virtual void ValidateAddress()
		{
			Address address = this.DefAddress.SelectSingle();
			if (address != null && address.IsValidated == false)
			{
				PXAddressValidator.Validate<Address>(Base, address, true, true);
			}
		}

		#endregion

		#region Events

		#region CacheAttached

		[PXDefault(ContactTypesAttribute.BAccountProperty)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<Contact.contactType> e) { }

		[PXDefault(PhoneTypesAttribute.Business2, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<Contact.phone2Type> e) { }

		[PXUIField(DisplayName = "Business Account", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDBDefault(typeof(BAccount.bAccountID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(BAccount.bAccountID), SubstituteKey = typeof(BAccount.acctCD), DirtyRead = true)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<Contact.bAccountID> e) { }

		#endregion

		#region Field-level

		protected virtual void _(Events.FieldUpdated<TMaster, FAcctName> e)
		{
			BAccount row = e.Row;

			if (this.DefContact.SelectSingle() is Contact defContact)
			{
				defContact.FullName = row.AcctName;

				this.DefContact.Update(defContact);
			}
		}

		protected virtual void _(Events.FieldUpdated<Address, Address.countryID> e)
		{
			Address addr = e.Row;

			if (!Base.IsContractBasedAPI && ((string)e.OldValue != addr.CountryID))
			{
				addr.State = null;
			}
		}

		#endregion

		#region Row-level

		protected virtual void _(Events.RowSelected<TMaster> e)
		{
			BAccount row = e.Row;
			if (row == null)
				return;

			ValidateAddresses.SetEnabled(e.Cache.GetStatus(row) != PXEntryStatus.Inserted);
		}

		protected virtual void _(Events.RowPersisting<Contact> e)
		{
			var row = e.Row;
			if (row == null || e.Operation != PXDBOperation.Update) return;

			var oldLang = (string)this.DefContact.Cache.GetValueOriginal<Contact.languageID>(row);
			if (oldLang == row.LanguageID)
				return;

			var account = Base.Caches[typeof(TMaster)].Current as TMaster;

			switch (account?.Type)
			{
				case BAccountType.CustomerType:

					// Acuminator disable once PX1043 SavingChangesInEventHandlers [legacy]
					PXDatabase.Update<AR.Customer>(
						new PXDataFieldAssign<AR.Customer.localeName>(row.LanguageID),
						new PXDataFieldRestrict<AR.Customer.bAccountID>(account?.BAccountID));

					break;

				case BAccountType.VendorType:

					// Acuminator disable once PX1043 SavingChangesInEventHandlers [legacy]
					PXDatabase.Update<AP.Vendor>(
						new PXDataFieldAssign<AP.Vendor.localeName>(row.LanguageID),
						new PXDataFieldRestrict<AP.Vendor.bAccountID>(account?.BAccountID));

					break;

				case BAccountType.CombinedType:

					// Acuminator disable once PX1043 SavingChangesInEventHandlers [legacy]
					PXDatabase.Update<AR.Customer>(
						new PXDataFieldAssign<AR.Customer.localeName>(row.LanguageID),
						new PXDataFieldRestrict<AR.Customer.bAccountID>(account?.BAccountID));

					// Acuminator disable once PX1043 SavingChangesInEventHandlers [legacy]
					PXDatabase.Update<AP.Vendor>(
						new PXDataFieldAssign<AP.Vendor.localeName>(row.LanguageID),
						new PXDataFieldRestrict<AP.Vendor.bAccountID>(account?.BAccountID));

					break;

			}
		}

		#endregion

		#endregion

		#region Inner Classes

		public abstract class WithPersistentAddressValidation : DefContactAddressExt<TGraph, TMaster, FAcctName>
		{
			protected override bool PersistentAddressValidation => true;
		}

		public abstract class WithCombinedTypeValidation : DefContactAddressExt<TGraph, TMaster, FAcctName>
		{
			#region Events

			protected virtual void _(Events.RowDeleting<TMaster> e)
			{
				BAccount row = e.Row;
				if (row == null)
					return;

				if (row != null && (row.Type == BAccountType.CombinedType || row.IsBranch == true))
				{
					PXParentAttribute.SetLeaveChildren<Contact.bAccountID>(this.DefContact.Cache, null, true);
					PXParentAttribute.SetLeaveChildren<Address.bAccountID>(this.DefAddress.Cache, null, true);
				}
			}

			#endregion
		}

		#endregion
	}
}
