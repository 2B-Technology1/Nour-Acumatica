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

using PX.Data;
using PX.Objects.AR.Standalone;
using PX.Objects.CR;
using PX.Objects.GL;
using System;
using System.Collections;
using System.Collections.Generic;
using static PX.Objects.CR.CROpportunity.FK;

namespace PX.Objects.AR
{
	public class ValidateARDocumentAddressProcess : ValidateDocumentAddressGraph<ValidateARDocumentAddressProcess>
	{
		#region Event Handlers
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXDBDefaultAttribute))]
		public virtual void _(Events.CacheAttached<ARShippingAddress.customerID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXDBDefaultAttribute))]
		public virtual void _(Events.CacheAttached<ARAddress.customerID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[DocumentTypeField.List(DocumentTypeField.SetOfValues.ARRelatedDocumentTypes)]
		public virtual void _(Events.CacheAttached<ValidateDocumentAddressFilter.documentType> e) { }
		#endregion

		#region Method Overrides
		protected override IEnumerable GetAddressRecords(ValidateDocumentAddressFilter filter)
		{
			IEnumerable result = new List<UnvalidatedAddress>();
			string docTypeFilter = filter.DocumentType.Trim();
			if (docTypeFilter.Equals(DocumentTypeField.InvoicesAndMemo))
			{
				result = AddARInvoiceAddresses(filter);
			}
			else if (docTypeFilter.Equals(DocumentTypeField.CashSales))
			{
				result = AddCashSalesAddresses(filter);
			}

			return result;
		}
		#endregion

		#region Protected methods
		protected virtual IEnumerable AddARInvoiceAddresses(ValidateDocumentAddressFilter filter)
		{
			List<object> shipAddresses = new List<object>();
			List<object> billAddresses = new List<object>();

			object[] statusList = new String[] { ARDocStatus.Hold, ARDocStatus.CreditHold, ARDocStatus.PendingPrint, ARDocStatus.PendingEmail, ARDocStatus.CCHold,
					ARDocStatus.Balanced, ARDocStatus.Scheduled, ARDocStatus.Open, ARDocStatus.Closed };
			object[] excludeDocTypes = new string[] { ARDocType.CashSale, ARDocType.CashReturn };
			object[] parms = new object[] { (object)statusList, (object)excludeDocTypes};

			PXSelectBase<ARInvoice> shipAddrCmd = new PXSelectJoin<ARInvoice,
				InnerJoin<Customer, On<ARInvoice.customerID, Equal<Customer.bAccountID>>,
				InnerJoin<ARShippingAddress, On<ARInvoice.shipAddressID, Equal<ARShippingAddress.addressID>>>>,
				Where<ARShippingAddress.isDefaultBillAddress, Equal<False>,
				And<ARShippingAddress.isValidated, Equal<False>,
				And<ARInvoice.released, Equal<False>,
				And<ARInvoice.status, In<Required<ARInvoice.status>>,
				And<ARInvoice.docType, NotIn<Required<ARInvoice.docType>>,
				And<ARInvoice.origModule, NotEqual<BatchModule.moduleSO>>>>>>>>(this);

			PXSelectBase<ARInvoice> billAddrCmd = new PXSelectJoin<ARInvoice,
				InnerJoin<Customer, On<ARInvoice.customerID, Equal<Customer.bAccountID>>,
				InnerJoin<ARAddress, On<ARInvoice.billAddressID, Equal<ARAddress.addressID>>>>,
				Where<ARAddress.isDefaultBillAddress, Equal<False>,
				And<ARAddress.isValidated, Equal<False>,
				And<ARInvoice.released, Equal<False>,
				And<ARInvoice.status, In<Required<ARInvoice.status>>,
				And<ARInvoice.docType, NotIn<Required<ARInvoice.docType>>,
				And<ARInvoice.origModule, NotEqual<BatchModule.moduleSO>>>>>>>>(this);

			if (!string.IsNullOrEmpty(filter?.Country))
			{
				parms = new object[] { (object)statusList, (object)excludeDocTypes, filter.Country };
				shipAddrCmd.WhereAnd<Where<ARShippingAddress.countryID, Equal<Required<ARShippingAddress.countryID>>>>();
				billAddrCmd.WhereAnd<Where<ARAddress.countryID, Equal<Required<ARAddress.countryID>>>>();
			}

			shipAddresses = shipAddrCmd.View.SelectMulti(parms);
			billAddresses = billAddrCmd.View.SelectMulti(parms);

			foreach (PXResult<ARInvoice, Customer, ARShippingAddress> item in shipAddresses)
			{
				var arAddress = (ARShippingAddress)item;
				var arInvoice = (ARInvoice)item;
				var docTypeLabel = new ARInvoiceType.ListAttribute().ValueLabelDic[arInvoice.DocType];

				UnvalidatedAddress address = ConvertToUnvalidatedAddress(arAddress, arInvoice,
					documentNbr: string.IsNullOrEmpty(docTypeLabel) ? arInvoice.RefNbr : string.Format("{0}, {1}", docTypeLabel, arInvoice.RefNbr),
					documentType: CR.MessagesNoPrefix.InvoicesAndMemoDesc,
					status: new ARDocStatus.ListAttribute().ValueLabelDic[arInvoice.Status]);

				yield return DocumentAddresses.Insert(address);
			}

			foreach (PXResult<ARInvoice, Customer, ARAddress> item in billAddresses)
			{
				var arAddress = (ARAddress)item;
				var arInvoice = (ARInvoice)item;
				var docTypeLabel = new ARInvoiceType.ListAttribute().ValueLabelDic[arInvoice.DocType];

				UnvalidatedAddress address = ConvertToUnvalidatedAddress(arAddress, arInvoice,
					documentNbr: string.IsNullOrEmpty(docTypeLabel) ? arInvoice.RefNbr : string.Format("{0}, {1}", docTypeLabel, arInvoice.RefNbr),
					documentType: CR.MessagesNoPrefix.InvoicesAndMemoDesc,
					status: new ARDocStatus.ListAttribute().ValueLabelDic[arInvoice.Status]);

				yield return DocumentAddresses.Insert(address);
			}
		}

		protected virtual IEnumerable AddCashSalesAddresses(ValidateDocumentAddressFilter filter)
		{
			List<object> shipAddresses = new List<object>();
			List<object> billAddresses = new List<object>();

			PXSelectBase<ARCashSale> billAddressCmd = new PXSelectJoin<ARCashSale,
					InnerJoin<ARAddress, On<ARCashSale.billAddressID, Equal<ARAddress.addressID>>>,
					Where<ARAddress.isDefaultBillAddress, Equal<False>, And<ARAddress.isValidated, Equal<False>,
					And<ARCashSale.released, Equal<False>,
					And<ARCashSale.origModule, NotEqual<BatchModule.moduleSO>>>>>>(this);

			PXSelectBase<ARCashSale> shipAddressCmd = new PXSelectJoin<ARCashSale,
					InnerJoin<ARShippingAddress, On<ARCashSale.shipAddressID, Equal<ARShippingAddress.addressID>>>,
					Where<ARShippingAddress.isDefaultBillAddress, Equal<False>, And<ARShippingAddress.isValidated, Equal<False>,
					And<ARCashSale.released, Equal<False>,
					And<ARCashSale.origModule, NotEqual<BatchModule.moduleSO>>>>>>(this);

			object[] parms = new object[] { };

			if (!string.IsNullOrEmpty(filter?.Country))
			{
				parms = new object[] { filter.Country };
				billAddressCmd.WhereAnd<Where<ARAddress.countryID, Equal<Required<ARAddress.countryID>>>>();
				shipAddressCmd.WhereAnd<Where<ARShippingAddress.countryID, Equal<Required<ARShippingAddress.countryID>>>>();
			}

			// For Cash Sales, Validate Addresses action is not presented in menu, include applicable statuses in query and select the records after fix.
			//billAddresses = billAddressCmd.View.SelectMulti(parms);
			//shipAddresses = shipAddressCmd.View.SelectMulti(parms);

			foreach (PXResult<ARCashSale, ARAddress> item in billAddresses)
			{
				var billToAddress = (ARAddress)item;
				var cashSale = (ARCashSale)item;
				string docTypeLabel = new ARCashSaleType.ListAttribute().ValueLabelDic[cashSale.DocType];

				UnvalidatedAddress address = ConvertToUnvalidatedAddress(billToAddress, cashSale,
					documentNbr: string.IsNullOrEmpty(docTypeLabel) ? cashSale.RefNbr : string.Format("{0}, {1}", docTypeLabel, cashSale.RefNbr),
					documentType: CR.MessagesNoPrefix.CashSalesDesc,
					status: new ARDocStatus.ListAttribute().ValueLabelDic[cashSale.Status]);

				yield return DocumentAddresses.Insert(address);
			}

			foreach (PXResult<ARCashSale, ARShippingAddress> item in shipAddresses)
			{
				var shipToAddress = (ARShippingAddress)item;
				var cashSale = (ARCashSale)item;
				string docTypeLabel = new ARCashSaleType.ListAttribute().ValueLabelDic[cashSale.DocType];

				UnvalidatedAddress address = ConvertToUnvalidatedAddress(shipToAddress, cashSale,
					documentNbr: string.IsNullOrEmpty(docTypeLabel) ? cashSale.RefNbr : string.Format("{0}, {1}", docTypeLabel, cashSale.RefNbr),
					documentType: CR.MessagesNoPrefix.CashSalesDesc,
					status: new ARDocStatus.ListAttribute().ValueLabelDic[cashSale.Status]);

				yield return DocumentAddresses.Insert(address);
			}
		}
		#endregion
	}
}
