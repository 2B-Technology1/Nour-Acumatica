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

using System.Linq;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.CR.Extensions;
using CRLocation = PX.Objects.CR.Standalone.Location;


namespace PX.Objects.CR
{
	public class BAccountCascadeAttribute : PXEventSubscriberAttribute, IPXRowDeletedSubscriber
	{
		public virtual void RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			var baccount = e.Row as BAccount;
			if (baccount == null
				|| baccount.Type == BAccountType.CombinedType
				|| baccount.IsBranch is true
					&& baccount.Type != BAccountType.BranchType) return;

			DetachNonAP(sender.Graph, baccount);

			// business Account data
			foreach (var rec in SelectFrom<BAccount>
					.LeftJoin<Contact>
						.On<Contact.contactID.IsEqual<BAccount.defContactID>>
					.LeftJoin<Address>
						.On<Address.addressID.IsEqual<BAccount.defAddressID>>
					.Where<BAccount.bAccountID.IsEqual<@P.AsInt>
							.And<Where<Contact.contactID.IsNotNull
									.Or<Address.addressID.IsNotNull>>>>
					.View
					.ReadOnly
					.Select(sender.Graph, baccount.BAccountID))
			{
				DeleteAP(sender.Graph, rec);
			}

			// billing
			foreach (var rec in SelectFrom<Customer>
					.LeftJoin<Contact>
						.On<Contact.contactID.IsEqual<Customer.defBillContactID>>
					.LeftJoin<Address>
						.On<Address.addressID.IsEqual<Customer.defBillAddressID>>
					.Where<Customer.bAccountID.IsEqual<@P.AsInt>
							.And<Where<Contact.contactID.IsNotNull
									.Or<Address.addressID.IsNotNull>>>>
					.View
					.ReadOnly
					.Select(sender.Graph, baccount.BAccountID))
			{
				DeleteAP(sender.Graph, rec);
			}

			// shipping
			foreach (var rec in SelectFrom<BAccount>
					.LeftJoin<CRLocation>
						.On<CRLocation.locationID.IsEqual<BAccount.defLocationID>>
					.LeftJoin<Contact>
						.On<Contact.contactID.IsEqual<CRLocation.defContactID>>
					.LeftJoin<Address>
						.On<Address.addressID.IsEqual<CRLocation.defAddressID>>
					.Where<BAccount.bAccountID.IsEqual<@P.AsInt>
							.And<Where<Contact.contactID.IsNotNull
									.Or<Address.addressID.IsNotNull>>>>
					.View
					.ReadOnly
					.Select(sender.Graph, baccount.BAccountID))
			{
				DeleteAP(sender.Graph, rec);
			}

			// remittance
			foreach (var rec in SelectFrom<BAccount>
					.LeftJoin<CRLocation>
						.On<CRLocation.locationID.IsEqual<BAccount.defLocationID>>
					.LeftJoin<Contact>
						.On<Contact.contactID.IsEqual<CRLocation.vRemitContactID>>
					.LeftJoin<Address>
						.On<Address.addressID.IsEqual<CRLocation.vRemitAddressID>>
					.Where<BAccount.bAccountID.IsEqual<@P.AsInt>
							.And<Where<Contact.contactID.IsNotNull
									.Or<Address.addressID.IsNotNull>>>>
					.View
					.ReadOnly
					.Select(sender.Graph, baccount.BAccountID))
			{
				DeleteAP(sender.Graph, rec);
			}

		}

		protected virtual void DetachNonAP(PXGraph graph, BAccount baccount)
		{
			var contacts =
				SelectFrom<Contact>
				.LeftJoin<Address>
					.On<Address.addressID.IsEqual<Contact.defAddressID>>
				.Where<Contact.bAccountID.IsEqual<@P.AsInt>
					.And<Contact.contactType.IsEqual<ContactTypesAttribute.person>>>
				.View
				.ReadOnly
				.Select(graph, baccount.BAccountID);
			foreach (var rec in contacts)
			{
				var contact = rec.GetItem<Contact>();
				var address = rec.GetItem<Address>();

				contact.BAccountID = null;
				graph.Caches[typeof(Contact)].Update(contact);
				address.BAccountID = null;
				graph.Caches[typeof(Address)].Update(address);
			}
		}

		protected virtual void DeleteAP(PXGraph graph, PXResult rec)
		{
			var contact = rec.GetItem<Contact>();
			var address = rec.GetItem<Address>();

			if (contact?.ContactID != null)
			{
				graph.Caches<Contact>().Delete(contact);
			}
			if (address?.AddressID != null)
			{
				graph.Caches<Address>().Delete(address);
			}
		}
	}
}
