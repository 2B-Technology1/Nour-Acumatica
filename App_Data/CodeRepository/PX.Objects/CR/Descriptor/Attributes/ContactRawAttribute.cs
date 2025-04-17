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
using System.Collections.Generic;
using System.Linq;
using PX.Api;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;

namespace PX.Objects.CR
{
	[PXDBInt]
	[PXInt]
	[PXUIField(DisplayName = "Contact", Visibility = PXUIVisibility.Visible)]
	[PXRestrictor(typeof(
		Where<
			Contact.isActive, Equal<True>>),
		Messages.ContactInactive, typeof(Contact.displayName))]
	public class ContactRawAttribute : PXEntityAttribute, IPXFieldDefaultingSubscriber, IPXFieldVerifyingSubscriber
	{
		#region State

		public virtual PXSelectorMode SelectorMode { get; set; } = PXSelectorMode.DisplayModeText;

		protected Type[] ContactTypes = new []{ typeof(ContactTypesAttribute.person) };

		protected Type BAccountIDField;

		public bool WithContactDefaultingByBAccount;

		protected PXView AvailableContacts;

		#endregion

		#region ctor

		public ContactRawAttribute()
			: this(null) { }

		public ContactRawAttribute(Type bAccountIDField)
			: this(bAccountIDField, null) { }

		public ContactRawAttribute(Type bAccountIDField = null, Type[] contactTypes = null, Type customSearchField = null, Type customSearchQuery = null, Type[] fieldList = null, string[] headerList = null)
		{
			BAccountIDField = bAccountIDField;

			ContactTypes = contactTypes ?? ContactTypes;

			PXSelectorAttribute attr =
				new PXSelectorAttribute(customSearchQuery ?? CreateSelect(bAccountIDField, customSearchField),
					fieldList: fieldList ?? new Type[]
					{
						typeof(Contact.displayName),
						typeof(Contact.salutation),
						typeof(Contact.fullName),
						typeof(BAccount.acctCD),
						typeof(Contact.eMail),
						typeof(Contact.phone1),
						typeof(Contact.contactType)
					})
				{
					Headers = headerList != null || fieldList != null // if field list custom and headerlist not provided - use defaults
						? headerList
						: new[]
					{
						"Contact",
						"Job Title",
						"Account Name",
						"Business Account",
						"Email",
						"Phone 1",
						"Type"
					},

					DescriptionField = typeof(Contact.displayName),
					SelectorMode = SelectorMode,
					Filterable = true,
					DirtyRead = true
				};

			_Attributes.Add(attr);

			_SelAttrIndex = _Attributes.Count - 1;

			if (bAccountIDField != null)
			{
				_Attributes.Add(new PXContactAccountDiffersRestrictorAttribute(BqlTemplate.OfCondition<
							Where<
								BqlPlaceholder.A.AsField.FromCurrent.IsNull
								.Or<Contact.bAccountID.IsEqual<BqlPlaceholder.A.AsField.FromCurrent>>>>
						.Replace<BqlPlaceholder.A>(bAccountIDField)
						.ToType(),
					messageParameters: new[]
					{
						attr.DescriptionField,
						BqlCommand.Compose(typeof(Selector<,>), bAccountIDField, typeof(BAccount.acctName))
					})
				{
					ShowWarning = true
				});
			}
		}

		protected virtual Type GetContactTypeWhere()
		{
			Type contactTypes = null;

			switch (ContactTypes.Length)
			{
				case 1:
					contactTypes = BqlCommand.Compose(
						typeof(Where<,>),
						typeof(Contact.contactType), typeof(Equal<>), ContactTypes[0]);
					break;
				case 2:
					contactTypes = BqlCommand.Compose(
						typeof(Where<,>),
						typeof(Contact.contactType), typeof(In3<,>), ContactTypes[0], ContactTypes[1]);
					break;
				case 3:
					contactTypes = BqlCommand.Compose(
						typeof(Where<,>),
						typeof(Contact.contactType), typeof(In3<,,>), ContactTypes[0], ContactTypes[1], ContactTypes[2]);
					break;
				case 4:
					contactTypes = BqlCommand.Compose(
						typeof(Where<,>),
						typeof(Contact.contactType), typeof(In3<,,,>), ContactTypes[0], ContactTypes[1], ContactTypes[2], ContactTypes[3]);
					break;
				case 5:
					contactTypes = BqlCommand.Compose(
						typeof(Where<,>),
						typeof(Contact.contactType), typeof(In3<,,,,>), ContactTypes[0], ContactTypes[1], ContactTypes[2], ContactTypes[3], ContactTypes[4]);
					break;
			}

			return contactTypes;
		}

		protected virtual Type CreateSelect(Type bAccountIDField, Type customSearchField)
		{
			Type contactTypes = GetContactTypeWhere();

			var employeeCondition = ContactTypes.Contains(typeof(ContactTypesAttribute.employee))
				? typeof(BAccount.isBranch.IsEqual<True>
					.And<Contact.contactType.IsEqual<ContactTypesAttribute.employee>>)
				: typeof(True.IsEqual<True>);

			return BqlTemplate.OfCommand<
					SelectFrom<
						Contact>
					.LeftJoin<BAccount>
						.On<BAccount.bAccountID.IsEqual<Contact.bAccountID>>
					.Where<
						Brackets<
							BAccount.bAccountID.IsNull
							.Or<Match<BAccount, Current<AccessInfo.userName>>>
						>
						.And<Brackets<
							BqlPlaceholder.C.AsCondition
							.Or<BAccount.type.IsNull>
							.Or<BAccount.type.IsIn<
								BAccountType.prospectType,
								BAccountType.customerType,
								BAccountType.combinedType,
								BAccountType.vendorType>>
						>>
						.And<BqlPlaceholder.A>
					>
					.SearchFor<BqlPlaceholder.B>>
				.Replace<BqlPlaceholder.A>(contactTypes)
				.Replace<BqlPlaceholder.B>(customSearchField ?? typeof(Contact.contactID))
				.Replace<BqlPlaceholder.C>(employeeCondition)
				.ToType();
		}

		#endregion

		#region Events

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			if (BAccountIDField != null)
			{
				Type contactTypes = GetContactTypeWhere();

				var cmd =
					BqlTemplate.OfCommand<
						SelectFrom<Contact>
							.LeftJoin<BAccount>
								.On<BAccount.bAccountID.IsEqual<Contact.bAccountID>
								.And<BAccount.defContactID.IsNotEqual<Contact.contactID>>>
							.Where<
								Contact.bAccountID.IsEqual<P.AsInt>
								.And<Contact.isActive.IsEqual<True>>
								.And<BqlPlaceholder.A>>>
					.Replace<BqlPlaceholder.A>(contactTypes)
					.ToCommand();

				AvailableContacts = new PXView(sender.Graph, true, cmd);

				sender.Graph.FieldUpdated.AddHandler(sender.GetItemType(), BAccountIDField.Name, BAccountID_FieldUpdated);
			}

			sender.Graph.OnBeforeCommit += (graph) =>
				{
				foreach (var row in sender.Inserted.Concat_(sender.Updated))
					{
					if ((int?)sender.GetValue(row, _FieldName) < 0)
						throw new PXException(Messages.NegativeValuedIdentifier, PXUIFieldAttribute.GetDisplayName(sender, _FieldName));
					}
			};
		}

		protected virtual void BAccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (e.Row == null
				|| BAccountIDField == null
				|| WithContactDefaultingByBAccount is false
				|| sender.Graph.UnattendedMode)
				return;

			var bAccountID = sender.GetValue(e.Row, BAccountIDField.Name);

			if (bAccountID == null || int.Equals(bAccountID, e.OldValue))
				return;

			var contactID = sender.GetValue(e.Row, this.FieldName) as int?;

			var contactsSet = GetAvailableContacts(bAccountID);

			if (contactsSet.Any(contact => contact.ContactID == contactID))
				return;

			sender.SetDefaultExt(e.Row, this.FieldName);
		}

		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row == null
				|| BAccountIDField == null
				|| WithContactDefaultingByBAccount is false
				|| sender.Graph.UnattendedMode)
				return;

			var bAccountID = sender.GetValue(e.Row, BAccountIDField.Name);

			if (bAccountID == null)
				return;

			var contactsSet = GetAvailableContacts(bAccountID);

			e.NewValue = contactsSet.Count == 1
				? contactsSet[0].ContactID
				: contactsSet.FirstOrDefault(item => item.IsPrimary || item.ContactID == e.NewValue as int?).ContactID;

			e.Cancel = true;
		}

		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.Row == null || BAccountIDField == null) return;

			// assuming that the source entity is in "legal" state
			if (sender.Graph.IsCopyPasteContext)
				e.Cancel = true;
		}

		#endregion

		#region Methods

		protected virtual List<(int? ContactID, bool IsPrimary)> GetAvailableContacts(object bAccountID)
		{
			return AvailableContacts
				?.SelectMulti(bAccountID)
				.Cast<PXResult<Contact, BAccount>>()
				.Select(item =>
				{
					var (contact, baccount) = item;
					var ContactID = contact.ContactID;
					var PrimaryContactID = baccount?.PrimaryContactID;
					return (ContactID, IsPrimary: PrimaryContactID == ContactID);
				})
				.ToList();
		}

		#endregion
	}
}
