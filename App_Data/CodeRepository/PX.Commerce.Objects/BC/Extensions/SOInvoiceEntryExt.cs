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

using PX.Commerce.Core;
using PX.Common;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.SO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Commerce.Objects
{
	public class BCSOInvoiceEntryExt : PXGraphExtension<SOInvoiceEntry>
	{
		public static bool IsActive() { return CommerceFeaturesHelper.CommerceEdition; }

		protected virtual void _(PX.Data.Events.RowInserted<ARTran> e)
		{
			ARTran row = e.Row;
			if (row == null)
				return;

			var soLine = SOLine.PK.Find(Base, row.SOOrderType, row.SOOrderNbr, row.SOOrderLineNbr);
			row.GetExtension<BCARTranExt>().AssociatedOrderLineNbr = soLine?.GetExtension<BCSOLineExt>()?.AssociatedOrderLineNbr;
			row.GetExtension<BCARTranExt>().GiftMessage = soLine?.GetExtension<BCSOLineExt>()?.GiftMessage;
		}
		public delegate void InvoiceOrderDelegate(InvoiceOrderArgs args);

		[PXOverride]
		public virtual void InvoiceOrder(InvoiceOrderArgs args, InvoiceOrderDelegate handler)
		{
			SOAddress soBillAddress = args.SoBillAddress;
			var pseudonymizationStatus = soBillAddress?.GetExtension<PX.Objects.GDPR.SOAddressExt>().PseudonymizationStatus;
			if (pseudonymizationStatus == PXPseudonymizationStatusListAttribute.Pseudonymized || pseudonymizationStatus == PXPseudonymizationStatusListAttribute.Erased)
				throw new PXException(BCMessages.CannotCreateInvoice);

			handler(args);
		}

		public delegate void PersistDelegate();
		[PXOverride]
		public void Persist(PersistDelegate handler)
		{
			UpdateIsEncryptedValue();
			handler();

		}

		public virtual void UpdateIsEncryptedValue()
		{
			foreach (ARInvoice doc in Base.Document.Cache.Inserted
							   .Concat_(Base.Document.Cache.Updated)
							   .Cast<ARInvoice>())
			{
				SOInvoice soInvoice = Base.SODocument.Select(doc.DocType, doc.RefNbr);
				SOOrderType orderType = null;
				if (soInvoice?.SOOrderType != null)
				{
					orderType = Base.soordertype.Select(soInvoice?.SOOrderType);
				}
				else
				{
					// for multiple order just get first and decide
					var type = Base.AllTransactions.Select(doc.DocType, doc.RefNbr).RowCast<ARTran>().ToList().Select(x => x.SOOrderType).FirstOrDefault();
					if (type != null)
						orderType = Base.soordertype.Select(type);
					else
					{
						//If there is no order attached to Invoice go to store and check the ordertype has Pii flag enabled.
						BCAPISyncScope.BCSyncScopeContext context = BCAPISyncScope.GetScoped();
						if (context != null)
						{
							var store = BCBindingExt.PK.Find(Base, context.Binding);
							if (store?.OrderType != null)
								orderType = Base.soordertype.Select(store.OrderType);
						}
					}
				}

				if (orderType != null) // if ordertype is null then cannot determine so dont change the value.
				{
					ARShippingContact shippingContact = Base.Shipping_Contact.Select(doc.ShipContactID);
					bool? overriden = null;
					if (shippingContact != null)
					{
						overriden = shippingContact?.OverrideContact;
						var shippingContactCopy = (ARShippingContact)Base.Shipping_Contact.Cache.CreateCopy(shippingContact);
						shippingContactCopy.IsEncrypted = SetEncryptionValue(overriden, orderType);
						Base.Shipping_Contact.Cache.Update(shippingContactCopy);
					}

					ARContact contact = Base.Billing_Contact.Select(doc.BillContactID);
					if (contact != null)
					{
						overriden = contact?.OverrideContact;
						var contactCopy = (ARContact)Base.Billing_Contact.Cache.CreateCopy(contact);
						contactCopy.IsEncrypted = SetEncryptionValue(overriden, orderType);
						Base.Billing_Contact.Cache.Update(contactCopy);
					}

					ARShippingAddress shippingAddress = Base.Shipping_Address.Select(doc.ShipAddressID);
					if (shippingAddress != null)
					{
						overriden = shippingAddress?.OverrideAddress;
						var shippingAddressCopy = (ARShippingAddress)Base.Shipping_Address.Cache.CreateCopy(shippingAddress);
						shippingAddressCopy.IsEncrypted = SetEncryptionValue(overriden, orderType);
						Base.Shipping_Address.Cache.Update(shippingAddressCopy);
					}

					ARAddress address = Base.Billing_Address.Select(doc.BillAddressID);
					if (address != null)
					{
						overriden = address?.OverrideAddress;
						var addressCopy = (ARAddress)Base.Billing_Address.Cache.CreateCopy(address);
						addressCopy.IsEncrypted = SetEncryptionValue(overriden, orderType);
						Base.Billing_Address.Cache.Update(addressCopy);
					}
				}
			}
		}

		protected virtual void ARAddress_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			ARAddress row = (ARAddress)e.Row;
			if (row == null)
				return;
			if (Base.Document.Current?.Released == true && row.AddressID == Base.Document.Current?.BillAddressID)
			{
				if (row.IsEncrypted == true)
					PXUIFieldAttribute.SetEnabled<ARAddress.overrideAddress>(cache, row, false);
			}

		}

		protected virtual void ARContact_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			ARContact row = (ARContact)e.Row;
			if (row == null)
				return;
			if (Base.Document.Current?.Released == true && row.ContactID == Base.Document.Current?.BillContactID)
			{
				if (row.IsEncrypted == true)
					PXUIFieldAttribute.SetEnabled<ARContact.overrideContact>(cache, row, false);
			}

		}
		private  bool SetEncryptionValue(bool? overriden, SOOrderType orderType)
		{
			if (overriden == null) return false;
			return overriden.Value && (orderType?.GetExtension<SOOrderTypeExt>()?.EncryptAndPseudonymizePII ?? false);

		}

	}
}
