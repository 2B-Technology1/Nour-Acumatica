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
using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;
using System.Collections.Generic;
using PX.Objects.GDPR;

namespace PX.Objects.CR.Extensions
{
	#region DACs

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	[PXHidden]
	[Serializable]
	public sealed class ContactExt : PXCacheExtension<Contact>
	{
		#region CanBeMadePrimary
		public abstract class canBeMadePrimary : PX.Data.BQL.BqlBool.Field<canBeMadePrimary> { }

		[PXBool]
		[PXUIField(DisplayName = "Can be made Primary", Visible = false, Visibility = PXUIVisibility.Invisible, Enabled = false)]
		//[PXFormula(typeof(True.When<Contact.isPrimary.IsEqual<False>>.Else<True>), Persistent = true, IsDirty = true)]
		[PXDependsOnFields(typeof(Contact.isPrimary), typeof(Contact.isActive))]
		public bool? CanBeMadePrimary =>
			Base.IsPrimary == false
			&& Base.IsActive == true;
		#endregion

		#region IsMeaningfull
		public abstract class isMeaningfull : PX.Data.BQL.BqlBool.Field<isMeaningfull> { }

		[PXBool]
		[PXDependsOnFields(
			typeof(Contact.firstName),
			typeof(Contact.lastName),
			typeof(Contact.salutation),
			typeof(Contact.eMail),
			typeof(Contact.phone1),
			typeof(Contact.phone2)
		)]
		[PXDBCalced(typeof(True), typeof(bool))]
		public bool? IsMeaningfull =>
			Base.FirstName != null
			|| Base.LastName != null
			|| Base.Salutation != null
			|| Base.EMail != null
			|| Base.Phone1 != null
			|| Base.Phone2 != null;
		#endregion
		
		#region IsAddedAsExt
		public abstract class isAddedAsExt : PX.Data.BQL.BqlBool.Field<isAddedAsExt> { }

		[PXBool]
		public bool? IsAddedAsExt { get; set; }
		#endregion
	}

	#endregion

	/// <exclude/>
	public abstract class CRPrimaryContactGraphExt<
			TGraph,
			TContactDetails,
			TMaster,
			FBAccountID,
			FPrimaryContactID> 
		: PXGraphExtension<TGraph>
			where TGraph : PXGraph
			where TContactDetails : PXGraphExtension<TGraph>
			where TMaster : BAccount, IBqlTable, new()
			where FBAccountID : BqlInt.Field<FBAccountID>
			where FPrimaryContactID : BqlInt.Field<FPrimaryContactID>
	{
		#region State

		public TContactDetails ContactDetailsExtension { get; private set; }

		#endregion

		#region Views

		[PXCopyPasteHiddenView]
		[PXViewName(Messages.PrimaryContact)]
		public SelectFrom<Contact>
			.Where<
				Contact.bAccountID.IsEqual<BqlInt.Field<FBAccountID>.FromCurrent>
				.And<Contact.contactType.IsEqual<ContactTypesAttribute.person>>
				.And<Contact.contactID.IsEqual<BqlInt.Field<FPrimaryContactID>.FromCurrent>>>
			.View
			PrimaryContactCurrent;

		protected PXView NonDirtyContactsGrid = null;

		protected abstract PXView ContactsView { get; }

		[Api.Export.PXOptimizationBehavior(IgnoreBqlDelegate = true)]
		protected IEnumerable primaryContactCurrent()
		{
			Contact contact = null;

			var account = Base.Caches[typeof(TMaster)].Current as TMaster;

			if (account?.PrimaryContactID != null)
			{
				using (new PXReadDeletedScope())
				{
					contact = PrimaryContactCurrent.View.QuickSelect().FirstOrDefault_() as Contact;
				}
			}

			if (contact == null
				&& account != null
				&& account.PrimaryContactID == null)
			{
				using (var r = new ReadOnlyScope(PrimaryContactCurrent.Cache, Base.Caches[typeof(TMaster)]))
				{
					contact = PrimaryContactCurrent.Insert();

					PrimaryContactCurrent.Cache.SetValueExt<ContactExt.isAddedAsExt>(contact, true);
					PrimaryContactCurrent.Cache.SetValue<Contact.contactType>(contact, ContactTypesAttribute.Person);
					PrimaryContactCurrent.Cache.SetValue<Contact.status>(contact, ContactStatus.Active);
					PrimaryContactCurrent.Cache.SetValue<Contact.defAddressID>(contact, account.DefAddressID);
					PrimaryContactCurrent.Cache.SetValue<Contact.phone2Type>(contact, PhoneTypesAttribute.Cell);
					PrimaryContactCurrent.Cache.SetValue<Contact.fullName>(contact, account.AcctName);

					account.PrimaryContactID = contact.ContactID;

					var prevStatus = Base.Caches[typeof(TMaster)].GetStatus(account);
					Base.Caches[typeof(TMaster)].Update(account);
					Base.Caches[typeof(TMaster)].SetStatus(account, prevStatus == PXEntryStatus.Notchanged ? PXEntryStatus.Held : prevStatus);
				}
			}

			if (contact != null)
			{
			SetUI(account, contact);
			}

			yield return contact;
		}

		#endregion

		#region ctor

		public override void Initialize()
		{
			base.Initialize();

			ContactDetailsExtension = Base.GetExtension<TContactDetails>()
					?? throw new PXException(Messages.GraphHaveNoExt, typeof(TContactDetails).Name);

			Base.Views[ContactsView.Name].WhereAnd<Where<ContactExt.isMeaningfull.IsEqual<True>>>();

			NonDirtyContactsGrid = new PXView(Base, true, Base.Views[ContactsView.Name].BqlSelect);
		}

		#endregion

		#region Actions

		public PXAction<TMaster> AddNewPrimaryContact;
		[PXUIField(DisplayName = Messages.AddNewContact, Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable addNewPrimaryContact(PXAdapter adapter)
		{
			if (Base.Caches[typeof(TMaster)].Current is TMaster account && account != null)
			{
				ContactMaint target = PXGraph.CreateInstance<ContactMaint>();

				Contact maincontact = target.Contact.Insert();

				maincontact.BAccountID = account.BAccountID;

				CRContactClass ocls = PXSelect<CRContactClass, Where<CRContactClass.classID, Equal<Current<Contact.classID>>>>
					.SelectSingleBound(Base, new object[] { maincontact });
				if (ocls?.DefaultOwner == CRDefaultOwnerAttribute.Source)
				{
					maincontact.WorkgroupID = account.WorkgroupID;
					maincontact.OwnerID = account.OwnerID;
				}

				maincontact = target.Contact.Update(maincontact);

				throw new PXRedirectRequiredException(target, true, "Contact") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}

			return adapter.Get();
		}

		public PXAction<TMaster> MakeContactPrimary;
		[PXUIField(DisplayName = Messages.SetAsPrimary)]
		[PXButton]
		public virtual void makeContactPrimary()
		{
			var account = Base.Caches[typeof(TMaster)].Current as TMaster;
			if (account == null || account.BAccountID == null) return;

			var row = ContactsView.Cache.Current as Contact;
			if (row == null || row.ContactID == null) return;

			var primaryContact = PrimaryContactCurrent.SelectSingle();
			if (primaryContact != null && PrimaryContactCurrent.Cache.GetStatus(primaryContact) == PXEntryStatus.Inserted)
			{
				PrimaryContactCurrent.Cache.Delete(primaryContact);
			}

			account.PrimaryContactID = row.ContactID;

			Base.Caches[typeof(TMaster)].Update(account);
		}

		#endregion

		#region Events

		protected virtual void _(Events.RowSelected<TMaster> e)
		{
			var row = e.Row as TMaster;
			if (row == null) return;

			var account = Base.Caches[typeof(TMaster)].Current;

			bool isContactsExists = NonDirtyContactsGrid?.SelectSingle() != null;

			PXUIFieldAttribute.SetVisible<FPrimaryContactID>(Base.Caches[typeof(TMaster)], account, isContactsExists);

			if (Base.IsContractBasedAPI)
				PXUIFieldAttribute.SetReadOnly<FPrimaryContactID>(e.Cache, row);
		}

		protected virtual void _(Events.RowSelected<Contact> e)
		{
			var row = e.Row as Contact;
			if (row == null) return;

			// Acuminator disable once PX1047 RowChangesInEventHandlersForbiddenForArgs [Just for UI]
			row.IsPrimary = false;
			TMaster acct = Base.Caches[typeof(TMaster)].Current as TMaster;
			if (acct == null) return;

			if (row.ContactID == acct.PrimaryContactID)
				// Acuminator disable once PX1047 RowChangesInEventHandlersForbiddenForArgs [Just for UI]
				row.IsPrimary = true;

			if (row.IsPrimary == true && Base.IsContractBasedAPI)
			{
				PXUIFieldAttribute.SetReadOnly<Contact.bAccountID>(e.Cache, row);
				PXUIFieldAttribute.SetReadOnly<Contact.contactID>(e.Cache, row);
			}
		}

		protected virtual void _(Events.RowDeleted<TMaster> e)
		{
			var row = e.Row as TMaster;
			if (row == null)
				return;

			if (e.Cache.GetStatus(row).IsIn(PXEntryStatus.Inserted, PXEntryStatus.InsertedDeleted))
				return;

			var primaryContact = PrimaryContactCurrent.SelectSingle();
			if (primaryContact == null || PrimaryContactCurrent.Cache.GetStatus(primaryContact) != PXEntryStatus.Inserted)
				return;

			PrimaryContactCurrent.Cache.Delete(primaryContact);
		}

		protected virtual void _(Events.FieldUpdated<FPrimaryContactID> e)
		{
			if (e.Row == null || e.OldValue == null) return;
			TMaster row = e.Row as TMaster;

			if (e.NewValue != null &&
				this.PrimaryContactCurrent.SelectSingle() is Contact primaryContact &&
				(row.AcctName != null && !row.AcctName.Equals(primaryContact.FullName)))
			{
				primaryContact.FullName = row.AcctName;

				this.PrimaryContactCurrent.Update(primaryContact);
			}

			Contact contact = PXSelect<Contact, Where<Contact.contactID, Equal<Required<Contact.contactID>>>>.SelectSingleBound(Base, null, e.OldValue);

			if (contact != null && ContactsView.Cache.GetStatus(contact) == PXEntryStatus.Inserted)
			{
				ContactsView.Cache.Delete(contact);
			}

			if (e.NewValue == null)
			{
				Base.SelectTimeStamp();
			}
		}

		protected virtual void _(Events.RowInserting<TMaster> e)
		{
			var row = e.Row;

			if (row == null)
				return;

			if (e.Cache.GetValue<FPrimaryContactID>(row) as int? < 0)
			{
				// It will be inserted properly in View delegate. No need to do it.
				// Plus, Cache.Extend will lead to PrimaryContactID < 0 here

				e.Cache.SetValue<FPrimaryContactID>(row, null);
			}
		}

		[PXOverride]
		public virtual void Persist(Action del)
		{
			var primaryContact = PrimaryContactCurrent.SelectSingle();

			if (primaryContact != null && PrimaryContactCurrent.Cache.GetStatus(primaryContact) == PXEntryStatus.Inserted)
			{
				var ext = PrimaryContactCurrent.Cache.GetExtension<ContactExt>(primaryContact);

				if (ext.IsMeaningfull != true)
				{
					PrimaryContactCurrent.Cache.Delete(primaryContact);

					Base.Caches[typeof(TMaster)].SetValue<FPrimaryContactID>(Base.Caches[typeof(TMaster)].Current, null);
				}
				else
				{
					UDFHelper.CopyAttributes(Base.Caches[typeof(TMaster)], null, PrimaryContactCurrent.Cache, primaryContact, primaryContact?.ClassID);
				}
			}
			
			//PX.Data.PXDBIdentityAttribute.FieldDefaulting works in such a way that if there is an entry in the cache with a new value (int.MinValue),
			//then the newly added entry will have a value +1. It does not matter what the status of the first record is, even if InsertedDeleted.
			//Thus, we add a record, click on the icon for adding files, Persist is called, on Persist we call the code above, the record will
			//receive the status InsertedDeleted, after which Select occurs and
			//the PX.Objects.CR.Extensions.CRPrimaryContactGraphExt.primaryContactCurrent code is called again, which creates a new entry in the cache,
			//now we have two new entries, one Inserted and the other InsertedDeleted. They differ only in the ContactID field.
			//If above someone remembered the first ContactID and when reloading the graph,
			//it will set it for the upper entity (for example, from here PX.Web.UI.PXSelectorBase.PerformSelect -> PX.Web.UI.PXSelectorBase.CreateSelectArgumentsExt),
			//then the code above will no longer work correctly, because a record with the Inserted status will not be found using PrimaryContactCurrent.SelectSingle().
			//Apparently, it will be correct to delete all non-significant inserted records (IsMeaningfull != true)
			foreach (var contact in PrimaryContactCurrent.Cache.Inserted)
			{
				var ext = PrimaryContactCurrent.Cache.GetExtension<ContactExt>(contact);
				if (ext.IsAddedAsExt == true && ext.IsMeaningfull != true)
				{
					PrimaryContactCurrent.Cache.Delete(contact);
				}
			}

			del();
		}

		#endregion

		#region Methods

		protected virtual void SetUI(TMaster account, Contact contact)
		{
			bool isRealContactExists = NonDirtyContactsGrid?.SelectSingle() != null;
			bool isRealContactSelected = (account?.PrimaryContactID > 0 && contact?.DeletedDatabaseRecord != true);
			bool isContactEditable = !isRealContactExists || isRealContactSelected;

			PXUIFieldAttribute.SetVisible<Contact.firstName>(PrimaryContactCurrent.Cache, contact, !isRealContactExists);
			PXUIFieldAttribute.SetVisible<Contact.lastName>(PrimaryContactCurrent.Cache, contact, !isRealContactExists);

			PXUIFieldAttribute.SetEnabled<Contact.salutation>(PrimaryContactCurrent.Cache, contact, isContactEditable);
			PXUIFieldAttribute.SetEnabled<Contact.eMail>(PrimaryContactCurrent.Cache, contact, isContactEditable);
			PXUIFieldAttribute.SetEnabled<Contact.phone1>(PrimaryContactCurrent.Cache, contact, isContactEditable);
			PXUIFieldAttribute.SetEnabled<Contact.phone1Type>(PrimaryContactCurrent.Cache, contact, isContactEditable);
			PXUIFieldAttribute.SetEnabled<Contact.phone2>(PrimaryContactCurrent.Cache, contact, isContactEditable);
			PXUIFieldAttribute.SetEnabled<Contact.phone2Type>(PrimaryContactCurrent.Cache, contact, isContactEditable);

			PXUIFieldAttribute.SetEnabled<Contact.consentAgreement>(PrimaryContactCurrent.Cache, contact, isContactEditable);
			PXUIFieldAttribute.SetEnabled<Contact.consentDate>(PrimaryContactCurrent.Cache, contact, isContactEditable);
			PXUIFieldAttribute.SetEnabled<Contact.consentExpirationDate>(PrimaryContactCurrent.Cache, contact, isContactEditable);

			PrimaryContactCurrent
				.Cache
				.GetAttributesOfType<PXConsentAgreementFieldAttribute>(contact, typeof(Contact.consentAgreement).Name)
				.ForEach(a => a.SuppressWarning = !isRealContactSelected);
				
		}

		[PXOverride]
		public virtual void CopyPasteGetScript(bool isImportSimple, List<PX.Api.Models.Command> script, List<PX.Api.Models.Container> containers)
		{
			int primarycontactIDIndex = script.FindIndex(_ => _.FieldName == nameof(BAccount.PrimaryContactID));
			if (primarycontactIDIndex == -1)
				return;

			Api.Models.Command cmdPrimaryContactID = script[primarycontactIDIndex];
			Api.Models.Container cntPrimaryContactID = containers[primarycontactIDIndex];

			script.Remove(cmdPrimaryContactID);
			containers.Remove(cntPrimaryContactID);
		}

		#endregion
	}
}
