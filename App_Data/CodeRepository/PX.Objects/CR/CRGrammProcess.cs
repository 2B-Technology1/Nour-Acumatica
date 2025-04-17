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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Common;
using PX.Common.Extensions;
using PX.Data;
using PX.Objects.CS;
using PX.SM;
using PX.Objects.CR.Extensions.CRDuplicateEntities;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;

namespace PX.Objects.CR
{
	[Serializable]
	public class CRGrammProcess : PXGraph<CRGrammProcess>
	{
		public PXCancel<Contact> Cancel;

		[PXHidden] 
		public PXSelect<BAccount> baccount;

		[PXViewDetailsButton(typeof(Contact))]
		[PXViewDetailsButton(typeof(Contact),
			typeof(Select<BAccount,
				Where<BAccount.bAccountID, Equal<Current<Contact.bAccountID>>>>))]
		public SelectFrom<Contact>
			.InnerJoin<CRGramValidationDateTime.ByLead>.On<True.IsEqual<True>>
			.InnerJoin<CRGramValidationDateTime.ByContact>.On<True.IsEqual<True>>
			.InnerJoin<CRGramValidationDateTime.ByBAccount>.On<True.IsEqual<True>>
			.LeftJoin<BAccount>
				.On<BAccount.defContactID.IsEqual<Contact.contactID>
					.And<BAccount.bAccountID.IsEqual<Contact.bAccountID>>>
			.LeftJoin<Address>
				.On<Contact.contactType.IsIn<ContactTypesAttribute.person, ContactTypesAttribute.lead>
					.And<Address.addressID.IsEqual<Contact.defAddressID>>>
			.LeftJoin<Address2>
				.On<Contact.contactType.IsEqual<ContactTypesAttribute.bAccountProperty>
					.And<Address2.addressID.IsEqual<BAccount.defAddressID>>>
			.LeftJoin<Location>
				.On<Location.bAccountID.IsEqual<BAccount.bAccountID>
				.And<Location.locationID.IsEqual<BAccount.defLocationID>>>
			.Where<
				Brackets<
					Contact.contactType.IsEqual<ContactTypesAttribute.lead>
						.And<Contact.grammValidationDateTime.IsLess<CRGramValidationDateTime.ByLead.value>>
					.Or<
						Contact.contactType.IsEqual<ContactTypesAttribute.person>
						.And<Contact.grammValidationDateTime.IsLess<CRGramValidationDateTime.ByContact.value>>
					>
					.Or<
						Contact.contactType.IsEqual<ContactTypesAttribute.bAccountProperty>
						.And<Contact.grammValidationDateTime.IsLess<CRGramValidationDateTime.ByBAccount.value>>
					>
				>
				.And<
					BAccount.bAccountID.IsNull
					.And<Contact.contactType.IsNotEqual<ContactTypesAttribute.bAccountProperty>>
					.Or<BAccount.type.IsIn<
							BAccountType.prospectType,
							BAccountType.customerType,
							BAccountType.vendorType,
							BAccountType.combinedType,
							BAccountType.empCombinedType
					>>
				>
			>
			.ProcessingView
			Items;

		public PXSetup<CRSetup> Setup;

		#region Ctors

		public CRGrammProcess()
		{
			// Acuminator disable once PX1057 PXGraphCreationDuringInitialization [legacy, not sure if it could be safely replaced with (this)]
			var processor = new CRGramProcessor();

			if (!processor.IsRulesDefined)
				throw new PXSetupNotEnteredException(ErrorMessages.SetupNotEntered, typeof(CRSetup), typeof(CRSetup).Name);

			Items.Cache.AllowInsert = true;

			PXUIFieldAttribute.SetDisplayName<Contact.displayName>(Items.Cache, Messages.Contact);

			Items.SetProcessDelegate((CRGrammProcess graph, Contact contact) =>
			{
				PersistGrams(graph, contact);
			});

			Items.ParallelProcessingOptions =
				settings =>
				{
					settings.IsEnabled = true;
				};

			Items.SuppressMerge = true;
			Items.SuppressUpdate = true;
		}

		#endregion	

		public static bool PersistGrams(PXGraph graph, Contact contact)
		{
			if (!PXAccess.FeatureInstalled<FeaturesSet.contactDuplicate>()) return false;

			var tail = new List<object>();

			graph.Views[nameof(Items)].AppendTail(contact, tail);

			if (contact != null && contact.ContactID > 1 && tail.Count >= 1)
			{
				var processor = new CRGramProcessor(graph);

				var result = processor.PersistGrams(
						new PXResult<Contact, Address, Location, BAccount>(
							contact,
							contact.ContactType == ContactTypesAttribute.BAccountProperty
								? ((PXResult)tail[0]).GetItem<Address2>()
								: ((PXResult)tail[0]).GetItem<Address>(),
							contact.ContactType == ContactTypesAttribute.BAccountProperty
								? ((PXResult)tail[0]).GetItem<Location>()
								: null,
							contact.ContactType == ContactTypesAttribute.BAccountProperty
								? ((PXResult)tail[0]).GetItem<BAccount>()
								: null
						)
					);

				contact.DuplicateStatus = result.NewDuplicateStatus;
				contact.GrammValidationDateTime = result.GramValidationDate;

				return result.IsGramsCreated;
			}

			return false;
		}
	}
}
