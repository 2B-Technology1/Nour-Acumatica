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

using PX.CS;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using PX.Data.BQL.Fluent;
using PX.Objects.CR.ContactMaint_Extensions;

namespace PX.Objects.CR.Extensions.CRCreateActions
{
	/// <exclude/>
	public abstract partial class CRCreateContactActionBase<TGraph, TMain>
		: CRCreateActionBase<
			TGraph,
			TMain,
			ContactMaint,
			Contact,
			ContactFilter,
			ContactConversionOptions>
		where TGraph : PXGraph, new()
		where TMain : class, IBqlTable, new()
	{
		#region Ctor
		protected override string TargetType => CRTargetEntityType.Contact;

		protected override ICRValidationFilter[] AdditionalFilters => new ICRValidationFilter[] { ContactInfoAttributes, ContactInfoUDF };

		#endregion

		#region Views

		[PXHidden]
		[PXCopyPasteHiddenView]
		public CRValidationFilter<ContactFilter> ContactInfo;
		protected override CRValidationFilter<ContactFilter> FilterInfo => ContactInfo;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public CRValidationFilter<PopupAttributes> ContactInfoAttributes;
		protected virtual IEnumerable contactInfoAttributes()
		{
			return GetFilledAttributes();
		}

		[PXHidden]
		[PXCopyPasteHiddenView]
		public CRValidationFilter<PopupUDFAttributes> ContactInfoUDF;
		protected virtual IEnumerable<PopupUDFAttributes> contactInfoUDF()
		{
			return GetRequiredUDFFields();
		}

		#endregion

		#region Events

		public virtual void _(Events.FieldUpdated<ContactFilter, ContactFilter.contactClass> e)
		{
			Base.Caches<PopupAttributes>().Clear();
		}

		#endregion

		#region Actions

		public PXAction<TMain> CreateContact;
		[PXUIField(DisplayName = Messages.CreateContact, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXButton(DisplayOnMainToolbar = false)]
		public virtual IEnumerable createContact(PXAdapter adapter)
		{
			if (AskExtConvert(throwOnException: false, out bool redirect))
			{
				if (Base.IsDirty)
					Base.Actions.PressSave();

				var processingGraph = Base.CloneGraphState();
				PXLongOperation.StartOperation(Base, () =>
				{
					var extension = processingGraph.GetProcessingExtension<CRCreateContactActionBase<TGraph, TMain>>();

					var result = extension.Convert();

					if (redirect)
						extension.Redirect(result);
				});
			}
			return adapter.Get();
		}

		// just dummy, required to close popup if commandname specified for Create and validation failed
		public PXAction<TMain> CreateContactCancel;
		[PXUIField(DisplayName = "Cancel", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXButton(DisplayOnMainToolbar = false)]
		public virtual IEnumerable createContactCancel(PXAdapter adapter)
		{
			return adapter.Get();
		}

		public PXAction<TMain> CreateContactFinish;
		[PXUIField(DisplayName = "Create", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXButton(DisplayOnMainToolbar = false)]
		public virtual IEnumerable createContactFinish(PXAdapter adapter)
		{
			if (Base.IsImport is false
				&& Base.IsContractBasedAPI is false)
			{
				bool valid = PopupValidator.TryValidate();
				if (valid is false)
					throw new PXActionInterruptException(Messages.ValidationFailedError);

			}

			return adapter.Get();
		}

		// just different DisplayName
		public PXAction<TMain> CreateContactFinishRedirect;
		[PXUIField(DisplayName = "Create and review", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXButton(DisplayOnMainToolbar = false)]
		public virtual IEnumerable createContactFinishRedirect(PXAdapter adapter)
		{
			return createContactFinish(adapter);
		}


		public PXAction<TMain> CreateContactToolBar;
		[PXUIField(DisplayName = Messages.CreateContact, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXButton(DisplayOnMainToolbar = false)]
		public virtual IEnumerable createContactToolBar(PXAdapter adapter)
		{
			return createContact(adapter);
		}

		public PXAction<TMain> CreateContactRedirect;
		[PXUIField(DisplayName = Messages.CreateContactRedirect, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXButton]
		public virtual IEnumerable createContactRedirect(PXAdapter adapter)
		{
			var graph = CreateTargetGraph();
			var entity = CreateMaster(graph, null);

			Redirect(new ConversionResult<Contact>()
				{
					Graph = graph,
					Entity = entity,
					Converted = false
				}
			);

			return adapter.Get();
		}

		protected override Contact CreateMaster(ContactMaint graph, ContactConversionOptions _)
		{
			var document = Documents.Current;
			var param = ContactInfo.Current;
			var docContact = Contacts.Current ?? Contacts.SelectSingle();
			var docAddress = Addresses.Current ?? Addresses.SelectSingle();

			Contact contact = new Contact
			{
				ContactType = ContactTypesAttribute.Person,
				ParentBAccountID = document.ParentBAccountID
			};

			MapContact(docContact, contact);
			MapConsentable(docContact, contact);
			MapFromFilter(param, contact);
			MapFromDocument(document, contact);

			contact = graph.Contact.Insert(contact);

			CRContactClass cls = PXSelect<
						CRContactClass,
					Where<
						CRContactClass.classID, Equal<Required<CRContactClass.classID>>>>
				.SelectSingleBound(graph, null, contact.ClassID);

			if (cls?.DefaultOwner == CRDefaultOwnerAttribute.Source)
			{
				contact.WorkgroupID = document.WorkgroupID;
				contact.OwnerID = document.OwnerID;
			}

			var address = graph.AddressCurrent.SelectSingle()
				?? throw new InvalidOperationException("Cannot get Address for Business Account."); // just to ensure

			MapAddress(docAddress, address);

			address = (Address)graph.AddressCurrent.Cache.Update(address);

			contact.DefAddressID = address.AddressID;

			contact = graph.Contact.Update(contact);

			ReverseDocumentUpdate(graph, contact);

			FillRelations(graph, contact);

			FillAttributes(graph.Answers, contact);

			FillUDF(ContactInfoUDF.Cache, Documents.Cache.GetMain(document), graph.Contact.Cache, contact, contact.ClassID);

			FillNotesAndAttachments(graph, Documents.Cache.GetMain(document), graph.Contact.Cache, contact);

			return graph.Contact.Update(contact);
		}

		protected override Contact MapFromDocument(Document source, Contact target)
		{
			target = base.MapFromDocument(source, target);

			target.ContactType = ContactTypesAttribute.Person;

			target.ParentBAccountID = source.ParentBAccountID;
			target.BAccountID = source.BAccountID;
			target.OverrideAddress = true;
			target.Source = source.Source;
			//target.IsActive = source.IsActive;
			target.LanguageID = source.LanguageID;
			target.OverrideSalesTerritory = source.OverrideSalesTerritory;
			if (target.OverrideSalesTerritory is true)
			{
				target.SalesTerritoryID = source.SalesTerritoryID;
			}

			return target;
		}

		protected override IPersonalContact MapContact(DocumentContact docContact, IPersonalContact target)
		{
			base.MapContact(docContact, target);

			target.Title = null;

			return target;
		}

		protected virtual void MapContactMethod(DocumentContactMethod source, Contact target)
		{
			if (source is null)
				throw new ArgumentNullException(nameof(source));
			if (target is null)
				throw new ArgumentNullException(nameof(target));

			target.Method = source.Method;
			target.NoFax = source.NoFax;
			target.NoMail = source.NoMail;
			target.NoMarketing = source.NoMarketing;
			target.NoCall = source.NoCall;
			target.NoEMail = source.NoEMail;
			target.NoMassMail = source.NoMassMail;
		}

		protected virtual void MapFromFilter(ContactFilter source, Contact target)
		{
			if (source is null)
				throw new ArgumentNullException(nameof(source));
			if (target is null)
				throw new ArgumentNullException(nameof(target));

			target.FirstName = source.FirstName;
			target.LastName = source.LastName;
			target.FullName = source.FullName;
			target.Salutation = source.Salutation;
			target.Phone1 = source.Phone1;
			target.Phone1Type = source.Phone1Type;
			target.Phone2 = source.Phone2;
			target.Phone2Type = source.Phone2Type;
			target.EMail = source.Email;
			target.ClassID = source.ContactClass;
		}

		protected override void OnBeforePersist(ContactMaint graph, Contact entity)
		{
			var dedupExt = graph.FindImplementation<ContactMaint.CRDuplicateEntitiesForContactGraphExt>();
			if (dedupExt == null)
				return;

			dedupExt.HardBlockOnly = true;
		}

		#endregion
	}

	/// <exclude/>
	public abstract partial class CRCreateContactAction<TGraph, TMain>
		: CRCreateContactActionBase<TGraph, TMain>
		where TGraph : PXGraph, new()
		where TMain : class, IBqlTable, new()
	{
		#region Views

		#region Document Contact Method Mapping
		protected class DocumentContactMethodMapping : IBqlMapping
		{
			public Type Extension => typeof(DocumentContactMethod);
			protected Type _table;
			public Type Table => _table;

			public DocumentContactMethodMapping(Type table)
			{
				_table = table;
			}
			public Type Method = typeof(DocumentContactMethod.method);
			public Type NoFax = typeof(DocumentContactMethod.noFax);
			public Type NoMail = typeof(DocumentContactMethod.noMail);
			public Type NoMarketing = typeof(DocumentContactMethod.noMarketing);
			public Type NoCall = typeof(DocumentContactMethod.noCall);
			public Type NoEMail = typeof(DocumentContactMethod.noEMail);
			public Type NoMassMail = typeof(DocumentContactMethod.noMassMail);
		}
		protected abstract DocumentContactMethodMapping GetDocumentContactMethodMapping();
		#endregion

		public PXSelectExtension<DocumentContactMethod> ContactMethod;

		public SelectFrom<
				Contact>
			.Where<
				Contact.contactID.IsEqual<Document.refContactID.FromCurrent.NoDefault>>
			.View
			ExistingContact;

		protected override IEnumerable<CSAnswers> GetAttributesForMasterEntity()
		{
			return ExistingContact.SelectSingle() is Contact contact
				? PXSelect<CSAnswers, Where<CSAnswers.refNoteID, Equal<Required<Contact.noteID>>>>
						.Select(Base, contact.NoteID).FirstTableItems
				: base.GetAttributesForMasterEntity();
		}

		protected override object GetMasterEntity()
		{
			return ExistingContact.SelectSingle();
		}
		#endregion

		#region Events

		protected virtual object GetDefaultFieldValueFromCache<TExistingField, TField>()
		{
			return ExistingContact.SelectSingle() is Contact existing
						&& existing.ContactID > 0
					? ExistingContact.Cache.GetValue(existing, typeof(TExistingField).Name)
					: Contacts.Cache.GetValue(Contacts.Current ?? Contacts.SelectSingle(), typeof(TField).Name);
		}

		public virtual void _(Events.FieldDefaulting<ContactFilter, ContactFilter.firstName> e)
		{
			e.NewValue = GetDefaultFieldValueFromCache<Contact.firstName, DocumentContact.firstName>();
		}
		public virtual void _(Events.FieldDefaulting<ContactFilter, ContactFilter.lastName> e)
		{
			e.NewValue = GetDefaultFieldValueFromCache<Contact.lastName, DocumentContact.lastName>();
		}
		public virtual void _(Events.FieldDefaulting<ContactFilter, ContactFilter.fullName> e)
		{
			e.NewValue = GetDefaultFieldValueFromCache<Contact.fullName, DocumentContact.fullName>();
		}
		public virtual void _(Events.FieldDefaulting<ContactFilter, ContactFilter.salutation> e)
		{
			e.NewValue = GetDefaultFieldValueFromCache<Contact.salutation, DocumentContact.salutation>();
		}
		public virtual void _(Events.FieldDefaulting<ContactFilter, ContactFilter.phone1> e)
		{
			e.NewValue = GetDefaultFieldValueFromCache<Contact.phone1, DocumentContact.phone1>();
		}
		public virtual void _(Events.FieldDefaulting<ContactFilter, ContactFilter.phone1Type> e)
		{
			e.NewValue = GetDefaultFieldValueFromCache<Contact.phone1Type, DocumentContact.phone1Type>();
		}
		public virtual void _(Events.FieldDefaulting<ContactFilter, ContactFilter.phone2> e)
		{
			e.NewValue = GetDefaultFieldValueFromCache<Contact.phone2, DocumentContact.phone2>();
		}
		public virtual void _(Events.FieldDefaulting<ContactFilter, ContactFilter.phone2Type> e)
		{
			e.NewValue = GetDefaultFieldValueFromCache<Contact.phone2Type, DocumentContact.phone2Type>();
		}
		public virtual void _(Events.FieldDefaulting<ContactFilter, ContactFilter.email> e)
		{
			e.NewValue = GetDefaultFieldValueFromCache<Contact.eMail, DocumentContact.email>();
		}

		public virtual void _(Events.RowSelected<ContactFilter> e)
		{
			var existing = ExistingContact.SelectSingle();

			e.Cache.AdjustUI(e.Row)
				.ForAllFields(_ => _.Enabled = existing == null);
		}

		public virtual void _(Events.RowSelected<Document> e)
		{
			var existing = ExistingContact.SelectSingle();
			CreateContact.SetEnabled(existing == null);
			if (existing != null)
				ContactInfoAttributes.AllowUpdate = ContactInfoUDF.AllowUpdate = false;
		}

		#endregion

		#region Actions

		public override ConversionResult<Contact> Convert(ContactConversionOptions options = null)
		{
			// do nothing if account already exist
			if (ExistingContact.SelectSingle() is Contact contact)
			{
				//PXTrace.WriteVerbose($"Using existing contact: {contact.ContactID}.");
				return new ConversionResult<Contact>
				{
					Entity = contact,
					Converted = false,
				};
			}
			var result = base.Convert(options);

			FillDependedRelation(result.Entity, options);

			return result;
		}

		protected override Contact CreateMaster(ContactMaint graph, ContactConversionOptions _)
		{
			var contact = base.CreateMaster(graph, _);

			var docContactMethod = ContactMethod.Current ?? ContactMethod.SelectSingle();
			MapContactMethod(docContactMethod, contact);

			TransferActivities(graph, contact);

			return graph.Contact.Update(contact);
		}

		/// <summary>
		/// Updates CRRelation view of related Graph with new ContactID if needed
		/// </summary>
		/// <param name="contact"></param>
		/// <param name="options"></param>
		protected virtual void FillDependedRelation(Contact contact, ContactConversionOptions options)
		{
			if (options?.GraphWithRelation != null)
			{
				var entity = Documents.Current;
				var relations = options.GraphWithRelation.Caches<CRRelation>()?.Cached;

				if (relations == null)
					return;

				foreach (CRRelation relation in relations)
				{
					if (relation.TargetNoteID == entity.NoteID
						&& relation.ContactID == null)
					{
						relation.ContactID = contact.ContactID;

						options.GraphWithRelation.Caches<CRRelation>().RaiseFieldUpdated<CRRelation.contactID>(relation, null);
						options.GraphWithRelation.Caches<CRRelation>().MarkUpdated(relation);

						options.GraphWithRelation.Actions.PressSave();
					}
				}
			}
		}
		protected override void ReverseDocumentUpdate(ContactMaint graph, Contact entity)
		{
			// need for right update Documents
			//Base.Caches<Contact>().SetStatus(entity, PXEntryStatus.Inserted);

			var doc = Documents.Current;
			Documents.Cache.SetValue<Document.refContactID>(doc, entity.ContactID);
			graph.Caches<TMain>().Update(GetMain(doc));

			var contact = Contacts.Current ?? Contacts.SelectSingle();
			Contacts.Cache.SetValue<DocumentContact.firstName>(contact, entity.FirstName);
			Contacts.Cache.SetValue<DocumentContact.lastName>(contact, entity.LastName);
			Contacts.Cache.SetValue<DocumentContact.fullName>(contact, entity.FullName);
			Contacts.Cache.SetValue<DocumentContact.salutation>(contact, entity.Salutation);
			Contacts.Cache.SetValue<DocumentContact.phone1>(contact, entity.Phone1);
			Contacts.Cache.SetValue<DocumentContact.phone1Type>(contact, entity.Phone1Type);
			Contacts.Cache.SetValue<DocumentContact.phone2>(contact, entity.Phone2);
			Contacts.Cache.SetValue<DocumentContact.phone2Type>(contact, entity.Phone2Type);
			Contacts.Cache.SetValue<DocumentContact.email>(contact, entity.EMail);
			var contactMain = Contacts.Cache.GetMain(contact);
			graph.Caches[contactMain.GetType()].Update(contactMain);
		}

		protected virtual void TransferActivities(ContactMaint graph, Contact contact)
		{
			foreach (CRPMTimeActivity activity in Activities.Select())
			{
				activity.ContactID = contact.ContactID;
				graph.GetExtension<ContactMaint_ActivityDetailsExt>().Activities.Update(activity);
			}
		}

		#endregion
	}
}
