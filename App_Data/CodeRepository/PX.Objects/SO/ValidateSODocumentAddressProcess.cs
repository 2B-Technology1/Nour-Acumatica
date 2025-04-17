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

using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PX.Objects.SO
{
	public class ValidateSODocumentAddressProcess : ValidateDocumentAddressGraph<ValidateSODocumentAddressProcess>
	{
		#region Event Handlers
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXDBDefaultAttribute))]
		public virtual void _(Events.CacheAttached<SOShippingAddress.customerID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXDBDefaultAttribute))]
		public virtual void _(Events.CacheAttached<SOBillingAddress.customerID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXDBDefaultAttribute))]
		public virtual void _(Events.CacheAttached<SOShipmentAddress.customerID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXDBDefaultAttribute))]
		public virtual void _(Events.CacheAttached<ARShippingAddress.customerID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXDBDefaultAttribute))]
		public virtual void _(Events.CacheAttached<ARAddress.customerID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[DocumentTypeField.List(DocumentTypeField.SetOfValues.SORelatedDocumentTypes)]
		public virtual void _(Events.CacheAttached<ValidateDocumentAddressFilter.documentType> e) { }
		#endregion

		#region Method Overrides
		protected override IEnumerable GetAddressRecords(ValidateDocumentAddressFilter filter)
		{
			IEnumerable result = new List<UnvalidatedAddress>();
			string docTypeFilter = filter.DocumentType.Trim();
			if (docTypeFilter.Equals(DocumentTypeField.SalesOrder))
			{
				result = AddSalesOrderAddresses(filter);
			}
			else if (docTypeFilter.Equals(DocumentTypeField.Shipments))
			{
				result = AddShipmentAddresses(filter);
			}
			else if (docTypeFilter.Equals(DocumentTypeField.Invoices))
			{
				result = AddSOInvoiceAddresses(filter);
			}

			return result;
		}
		#endregion

		#region Other protected methods
		protected virtual IEnumerable AddSalesOrderAddresses(ValidateDocumentAddressFilter filter)
		{
			// For Sales Order, the Validate Addresses is present in actions differently depending upon the SO behavior assigned to the Order Type.
			object[] cMBehaviorStatuses = new String[] { SOOrderStatus.Hold, SOOrderStatus.Open, SOOrderStatus.Invoiced, SOOrderStatus.Completed, SOOrderStatus.Cancelled };
			object[] iNAndMOBehaviorStatuses = new String[] { SOOrderStatus.Hold, SOOrderStatus.CreditHold, SOOrderStatus.Open, SOOrderStatus.Invoiced,
					SOOrderStatus.Completed, SOOrderStatus.Cancelled };
			object[] qTBehaviorStatuses = new String[] { SOOrderStatus.Hold, SOOrderStatus.Open, SOOrderStatus.Completed, SOOrderStatus.Cancelled };
			object[] rMBehaviorStatuses = new String[] { SOOrderStatus.Hold, SOOrderStatus.CreditHold, SOOrderStatus.Open, SOOrderStatus.Completed, SOOrderStatus.Cancelled };
			object[] sOBehaviorStatuses = new String[] { SOOrderStatus.Hold, SOOrderStatus.CreditHold, SOOrderStatus.PendingProcessing, SOOrderStatus.AwaitingPayment, SOOrderStatus.Open,
					SOOrderStatus.BackOrder, SOOrderStatus.Completed, SOOrderStatus.Cancelled};
			object[] tRBehaviorStatuses = new String[] { SOOrderStatus.Hold, SOOrderStatus.Open, SOOrderStatus.BackOrder, SOOrderStatus.Completed, SOOrderStatus.Cancelled };

			Dictionary<string, object[]> statusesForSOBehavior = new Dictionary<string, object[]>
				{
					{ SOBehavior.CM, cMBehaviorStatuses },
					{ SOBehavior.IN, iNAndMOBehaviorStatuses },
					{ SOBehavior.MO, iNAndMOBehaviorStatuses },
					{ SOBehavior.QT, qTBehaviorStatuses },
					{ SOBehavior.RM, rMBehaviorStatuses },
					{ SOBehavior.SO, sOBehaviorStatuses },
					{ SOBehavior.TR, tRBehaviorStatuses },
				};
			List<Object> soShipAddresses = new List<Object>();
			List<Object> soBillAddresses = new List<Object>();

			PXSelectBase<SOOrder> shipAddrCmd = new PXSelectJoin<SOOrder,
					InnerJoin<SOShippingAddress, On<SOOrder.shipAddressID, Equal<SOShippingAddress.addressID>>>,
					Where<SOShippingAddress.isDefaultAddress, Equal<False>, And<SOShippingAddress.isValidated, Equal<False>,
					And<SOOrder.completed, Equal<False>, And<SOOrder.cancelled, Equal<False>,
					And<SOOrder.behavior, Equal<Required<SOOrder.behavior>>,
					And<SOOrder.status, In<Required<SOOrder.status>>>>>>>>>(this);

			PXSelectBase<SOOrder> billAddrCmd = new PXSelectJoin<SOOrder,
				InnerJoin<SOBillingAddress, On<SOOrder.billAddressID, Equal<SOBillingAddress.addressID>>>,
				Where<SOBillingAddress.isDefaultAddress, Equal<False>, And<SOBillingAddress.isValidated, Equal<False>,
				And<SOOrder.completed, Equal<False>, And<SOOrder.cancelled, Equal<False>,
				And<SOOrder.behavior, Equal<Required<SOOrder.behavior>>,
				And<SOOrder.status, In<Required<SOOrder.status>>>>>>>>>(this);

			if (!string.IsNullOrEmpty(filter?.Country))
			{
				shipAddrCmd.WhereAnd<Where<SOShippingAddress.countryID, Equal<Required<SOShippingAddress.countryID>>>>();
				billAddrCmd.WhereAnd<Where<SOBillingAddress.countryID, Equal<Required<SOBillingAddress.countryID>>>>();
			}
			foreach (var val in statusesForSOBehavior)
			{
				object[] parms = string.IsNullOrEmpty(filter?.Country) ? new object[] { val.Key, (object)val.Value } : new object[] { val.Key, (object)val.Value, filter.Country };
				soShipAddresses.AddRange(shipAddrCmd.View.SelectMulti(parms));
				soBillAddresses.AddRange(billAddrCmd.View.SelectMulti(parms));
			}

			foreach (PXResult<SOOrder, SOShippingAddress> item in soShipAddresses)
			{
				var soAddress = (SOShippingAddress)item;
				var soOrder = (SOOrder)item;

				UnvalidatedAddress address = ConvertToUnvalidatedAddress(soAddress, soOrder,
					documentNbr: string.Format("{0}, {1}", soOrder.OrderType, soOrder.OrderNbr),
					documentType: CR.MessagesNoPrefix.SalesOrderDesc,
					status: new SOOrderStatus.ListAttribute().ValueLabelDic[soOrder.Status]);

				yield return DocumentAddresses.Insert(address);
			}

			foreach (PXResult<SOOrder, SOBillingAddress> item in soBillAddresses)
			{
				var soAddress = (SOBillingAddress)item;
				var soOrder = (SOOrder)item;

				UnvalidatedAddress address = ConvertToUnvalidatedAddress(soAddress, soOrder,
					documentNbr: string.Format("{0}, {1}", soOrder.OrderType, soOrder.OrderNbr),
					documentType: CR.MessagesNoPrefix.SalesOrderDesc,
					status: new SOOrderStatus.ListAttribute().ValueLabelDic[soOrder.Status]);

				yield return DocumentAddresses.Insert(address);
			}
		}

		protected virtual IEnumerable AddShipmentAddresses(ValidateDocumentAddressFilter filter)
		{
			object[] statusList = new String[] { SOShipmentStatus.Hold, SOShipmentStatus.Open, SOShipmentStatus.Confirmed,
					SOShipmentStatus.PartiallyInvoiced, SOShipmentStatus.Invoiced};
			List<Object> soShipAddresses = new List<object>();

			PXSelectBase<SOShipment> shipAddrCmd = new PXSelectJoin<SOShipment,
					InnerJoin<SOShipmentAddress, On<SOShipment.shipAddressID, Equal<SOShipmentAddress.addressID>>>,
					Where<SOShipmentAddress.isDefaultAddress, Equal<False>, And<SOShipmentAddress.isValidated, Equal<False>,
					And<SOShipment.confirmed, Equal<False>,
					And<SOShipment.status, In<Required<SOShipment.status>>>>>>>(this);

			object[] parms = new object[] { (object)statusList };

			if (!string.IsNullOrEmpty(filter?.Country))
			{
				parms = new object[] { (object)statusList, filter.Country };
				shipAddrCmd.WhereAnd<Where<SOShipmentAddress.countryID, Equal<Required<SOShipmentAddress.countryID>>>>();
			}

			soShipAddresses = shipAddrCmd.View.SelectMulti(parms);

			foreach (PXResult<SOShipment, SOShipmentAddress> item in soShipAddresses)
			{
				var shipToAddress = (SOShipmentAddress)item;
				var shipment = (SOShipment)item;
				string shipmentTypeLabel = new SOShipmentType.ListAttribute().ValueLabelDic[shipment.ShipmentType];

				UnvalidatedAddress address = ConvertToUnvalidatedAddress(shipToAddress, shipment,
					documentNbr: string.IsNullOrEmpty(shipmentTypeLabel) ? shipment.ShipmentNbr : string.Format("{0}, {1}", shipmentTypeLabel, shipment.ShipmentNbr),
					documentType: CR.MessagesNoPrefix.ShipmentsDesc,
					status: new SOShipmentStatus.ListAttribute().ValueLabelDic[shipment.Status]);

				yield return DocumentAddresses.Insert(address);
			}
		}

		protected virtual IEnumerable AddSOInvoiceAddresses(ValidateDocumentAddressFilter filter)
		{
			object[] statusList = new String[] { ARDocStatus.Hold, ARDocStatus.CCHold, ARDocStatus.CreditHold, ARDocStatus.PendingPrint,
					ARDocStatus.PendingEmail, ARDocStatus.Balanced, ARDocStatus.Open, ARDocStatus.Closed };
			List<Object> shipAddresses = new List<object>();
			List<Object> billAddresses = new List<object>();

			PXSelectBase<SOInvoice> shipAddrCmd = new PXSelectJoin<SOInvoice,
					InnerJoin<ARInvoice, On<SOInvoice.refNbr, Equal<ARInvoice.refNbr>, And<SOInvoice.docType, Equal<ARInvoice.docType>>>,
					InnerJoin<Customer, On<ARInvoice.customerID, Equal<Customer.bAccountID>>,
					InnerJoin<ARShippingAddress, On<ARInvoice.shipAddressID, Equal<ARShippingAddress.addressID>>>>>,
					Where<ARShippingAddress.isDefaultBillAddress, Equal<False>, And<ARShippingAddress.isValidated, Equal<False>,
					And<SOInvoice.released, Equal<False>,
					And<ARInvoice.origModule, Equal<BatchModule.moduleSO>,
					And<SOInvoice.status, In<Required<SOInvoice.status>>>>>>>>(this);

			PXSelectBase<SOInvoice> billAddrCmd = new PXSelectJoin<SOInvoice,
					InnerJoin<ARInvoice, On<SOInvoice.refNbr, Equal<ARInvoice.refNbr>, And<SOInvoice.docType, Equal<ARInvoice.docType>>>,
					InnerJoin<Customer, On<ARInvoice.customerID, Equal<Customer.bAccountID>>,
					InnerJoin<ARAddress, On<ARInvoice.billAddressID, Equal<ARAddress.addressID>>>>>,
					Where<ARAddress.isDefaultBillAddress, Equal<False>, And<ARAddress.isValidated, Equal<False>,
					And<SOInvoice.released, Equal<False>,
					And<ARInvoice.origModule, Equal<BatchModule.moduleSO>,
					And<SOInvoice.status, In<Required<SOInvoice.status>>>>>>>>(this);

			object[] parms = new object[] { (object)statusList };

			if (!string.IsNullOrEmpty(filter?.Country))
			{
				parms = new object[] { (object)statusList, filter.Country };
				shipAddrCmd.WhereAnd<Where<ARShippingAddress.countryID, Equal<Required<ARShippingAddress.countryID>>>>();
				billAddrCmd.WhereAnd<Where<ARAddress.countryID, Equal<Required<ARAddress.countryID>>>>();
			}

			shipAddresses = shipAddrCmd.View.SelectMulti(parms);
			billAddresses = billAddrCmd.View.SelectMulti(parms);

			foreach (PXResult<SOInvoice, ARInvoice, Customer, ARShippingAddress> item in shipAddresses)
			{
				var soInvoiceShipAddr = (ARShippingAddress)item;
				var soInvoice = (ARInvoice)item;
				string docTypeLabel = new ARDocType.ListAttribute().ValueLabelDic[soInvoice.DocType];

				UnvalidatedAddress address = ConvertToUnvalidatedAddress(soInvoiceShipAddr, soInvoice,
					documentNbr: string.IsNullOrEmpty(docTypeLabel) ? soInvoice.RefNbr : string.Format("{0}, {1}", docTypeLabel, soInvoice.RefNbr),
					documentType: CR.MessagesNoPrefix.InvoicesDesc,
					status: new ARDocStatus.ListAttribute().ValueLabelDic[soInvoice.Status]);

				yield return DocumentAddresses.Insert(address);
			}

			foreach (PXResult<SOInvoice, ARInvoice, Customer, ARAddress> item in billAddresses)
			{
				var soInvoiceBillAddr = (ARAddress)item;
				var soInvoice = (ARInvoice)item;
				UnvalidatedAddress doc = new UnvalidatedAddress();
				string docTypeLabel = new ARDocType.ListAttribute().ValueLabelDic[soInvoice.DocType];

				UnvalidatedAddress address = ConvertToUnvalidatedAddress(soInvoiceBillAddr, soInvoice,
					documentNbr: string.IsNullOrEmpty(docTypeLabel) ? soInvoice.RefNbr : string.Format("{0}, {1}", docTypeLabel, soInvoice.RefNbr),
					documentType: CR.MessagesNoPrefix.InvoicesDesc,
					status: new ARDocStatus.ListAttribute().ValueLabelDic[soInvoice.Status]);

				yield return DocumentAddresses.Insert(address);
			}
		}
		#endregion
	}
}
