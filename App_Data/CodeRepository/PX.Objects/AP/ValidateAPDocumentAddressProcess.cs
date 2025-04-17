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
using PX.Objects.CR;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PX.Objects.AP
{
	public class ValidateAPDocumentAddressProcess : ValidateDocumentAddressGraph<ValidateAPDocumentAddressProcess>
	{
		#region Event Handlers
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXDBDefaultAttribute))]
		public virtual void _(Events.CacheAttached<APAddress.vendorID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[DocumentTypeField.List(DocumentTypeField.SetOfValues.APRelatedDocumentTypes)]
		public virtual void _(Events.CacheAttached<ValidateDocumentAddressFilter.documentType> e) { }
		#endregion

		#region Method Overrides
		protected override IEnumerable GetAddressRecords(ValidateDocumentAddressFilter filter)
		{
			IEnumerable result = new List<UnvalidatedAddress>();
			if (filter.DocumentType.Trim().Equals(DocumentTypeField.ChecksAndPayments))
			{
				result = AddAPPaymentAddresses(filter);
			}
			return result;
		}
		#endregion

		#region Protected Methods
		protected virtual IEnumerable AddAPPaymentAddresses(ValidateDocumentAddressFilter filter)
		{
			object[] statusList = new String[] { APDocStatus.Hold, APDocStatus.PendingPrint, APDocStatus.Printed, APDocStatus.Balanced };
			object[] docTypeList = new String[] { APDocType.Check, APDocType.Prepayment, APDocType.Refund, APDocType.VoidRefund, APDocType.VoidCheck, APDocType.DebitAdj };
			object[] parms = new object[] { (object)statusList, (object)docTypeList };
			List<Object> remittanceAddress = null;

			PXSelectBase<APPayment> remitAddrCmd = new PXSelectJoin<APPayment,
					InnerJoin<APAddress, On<APPayment.remitAddressID, Equal<APAddress.addressID>>>,
					Where<APAddress.isDefaultAddress, Equal<False>, And<APAddress.isValidated, Equal<False>,
					And<APPayment.released, Equal<False>,
					And<APPayment.status, In<Required<APPayment.status>>,
					And<APPayment.docType, In<Required<APPayment.docType>>>>>>>>(this);

			if (!string.IsNullOrEmpty(filter?.Country))
			{
				parms = new object[] { (object)statusList, (object)docTypeList, filter.Country };
				remitAddrCmd.WhereAnd<Where<APAddress.countryID, Equal<Required<APAddress.countryID>>>>();
			}

			remittanceAddress = remitAddrCmd.View.SelectMulti(parms);

			foreach (PXResult<APPayment, APAddress> item in remittanceAddress)
			{
				var apAddress = (APAddress)item;
				var apPayment = (APPayment)item;
				string docTypeLabel = new APPaymentType.ListAttribute().ValueLabelDic[apPayment.DocType];

				UnvalidatedAddress address = ConvertToUnvalidatedAddress(apAddress, apPayment,
					documentNbr: string.IsNullOrEmpty(docTypeLabel) ? apPayment.RefNbr : string.Format("{0}, {1}", docTypeLabel, apPayment.RefNbr),
					documentType: CR.MessagesNoPrefix.ChecksAndPaymentsDesc,
					status: new APDocStatus.ListAttribute().ValueLabelDic[apPayment.Status]);

				yield return DocumentAddresses.Insert(address);
			}
		}
		#endregion

	}
}
