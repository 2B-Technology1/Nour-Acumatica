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
using PX.Objects.AP;
using PX.Objects.CR.Standalone;
using PX.Objects.PM;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PX.Objects.CR
{
	public class ValidateCRDocumentAddressProcess : ValidateDocumentAddressGraph<ValidateCRDocumentAddressProcess>
	{
		#region Event Handlers
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[DocumentTypeField.List(DocumentTypeField.SetOfValues.CRRelatedDocumentTypes)]
		public virtual void _(Events.CacheAttached<ValidateDocumentAddressFilter.documentType> e) { }
		#endregion

		#region Method Overrides
		protected override IEnumerable GetAddressRecords(ValidateDocumentAddressFilter filter)
		{
			IEnumerable result = new List<UnvalidatedAddress>();
			string docTypeFilter = filter.DocumentType.Trim();

			if (docTypeFilter.Equals(DocumentTypeField.SalesQuote))
			{
				result = AddCRQuoteAddresses(filter);
			}
			else if (docTypeFilter.Equals(DocumentTypeField.Opportunities))
			{
				result = AddCROpportunityAddresses(filter);
			}
			else if (docTypeFilter.Equals(DocumentTypeField.ProjectsQuote))
			{
				result = AddPMQuoteAddresses(filter);
			}

			return result;
		}
		#endregion

		#region Protected Methods
		protected virtual IEnumerable AddCRQuoteAddresses(ValidateDocumentAddressFilter filter)
		{
			List<object> crShipAddresses = new List<object>();
			List<object> crBillAddresses = new List<object>();
			List<object> crQuoteAddresses = new List<object>();
			object[] parms = new object[] { };

			PXSelectBase<CRQuote> defaultCmdShipAddr = new PXSelectJoin<CRQuote,
				InnerJoin<CRShippingAddress, On<CRQuote.shipAddressID, Equal<CRShippingAddress.addressID>>>,
				Where<CRQuote.quoteType, Equal<CRQuoteTypeAttribute.distribution>,
				And<CRShippingAddress.isDefaultAddress, Equal<False>,
				And<CRShippingAddress.isValidated, Equal<False>>>>>(this);

			PXSelectBase<CRQuote> defaultCmdBillAddr = new PXSelectJoin<CRQuote,
				InnerJoin<CRBillingAddress, On<CRQuote.billAddressID, Equal<CRBillingAddress.addressID>>>,
				Where<CRQuote.quoteType, Equal<CRQuoteTypeAttribute.distribution>,
				And<CRBillingAddress.isDefaultAddress, Equal<False>,
				And<CRBillingAddress.isValidated, Equal<False>>>>>(this);

			PXSelectBase<CRQuote> defaultCmdQuoteAddr = new PXSelectJoin<CRQuote,
				InnerJoin<CRAddress, On<CRQuote.opportunityAddressID, Equal<CRAddress.addressID>>>,
				Where<CRQuote.quoteType, Equal<CRQuoteTypeAttribute.distribution>,
				And<CRAddress.isValidated, Equal<False>,
				And<Where<CRAddress.isDefaultAddress, Equal<False>,
				Or<CRQuote.bAccountID, IsNull, And<CRQuote.contactID, IsNull>>>>>>>(this);

			if (!string.IsNullOrEmpty(filter?.Country))
			{
				parms = new object[] { filter.Country };
				defaultCmdShipAddr.WhereAnd<Where<CRShippingAddress.countryID, Equal<Required<CRShippingAddress.countryID>>>>();
				defaultCmdBillAddr.WhereAnd<Where<CRBillingAddress.countryID, Equal<Required<CRBillingAddress.countryID>>>>();
				defaultCmdQuoteAddr.WhereAnd<Where<CRAddress.countryID, Equal<Required<CRAddress.countryID>>>>();
			}

			crShipAddresses = defaultCmdShipAddr.View.SelectMulti(parms);
			crBillAddresses = defaultCmdBillAddr.View.SelectMulti(parms);
			crQuoteAddresses = defaultCmdQuoteAddr.View.SelectMulti(parms);

			foreach (PXResult<CRQuote, CRShippingAddress> item in crShipAddresses)
			{
				var crshipaddr = (CRShippingAddress)item;
				var crQuote = (CRQuote)item;

				UnvalidatedAddress address = ConvertToUnvalidatedAddress(crshipaddr, crQuote,
					documentNbr: crQuote.QuoteNbr,
					documentType: CR.MessagesNoPrefix.SalesQuoteDesc,
					status: new PMQuoteStatusAttribute().ValueLabelDic[crQuote.Status]);

				yield return DocumentAddresses.Insert(address);
			}

			foreach (PXResult<CRQuote, CRBillingAddress> item in crBillAddresses)
			{
				var crBillAddr = (CRBillingAddress)item;
				var crQuote = (CRQuote)item;

				UnvalidatedAddress address = ConvertToUnvalidatedAddress(crBillAddr, crQuote,
					documentNbr: crQuote.QuoteNbr,
					documentType: CR.MessagesNoPrefix.SalesQuoteDesc,
					status: new PMQuoteStatusAttribute().ValueLabelDic[crQuote.Status]);

				yield return DocumentAddresses.Insert(address);
			}

			foreach (PXResult<CRQuote, CRAddress> item in crQuoteAddresses)
			{
				var crAddress = (CRAddress)item;
				var crQuote = (CRQuote)item;

				UnvalidatedAddress address = ConvertToUnvalidatedAddress(crAddress, crQuote,
					documentNbr: crQuote.QuoteNbr,
					documentType: CR.MessagesNoPrefix.SalesQuoteDesc,
					status: new PMQuoteStatusAttribute().ValueLabelDic[crQuote.Status]);

				yield return DocumentAddresses.Insert(address);
			}
		}

		protected virtual IEnumerable AddCROpportunityAddresses(ValidateDocumentAddressFilter filter)
		{
			List<object> crContactAddresses = new List<object>();
			List<object> crShipAddresses = new List<object>();
			List<object> crBillAddresses = new List<object>();
			object[] statusList = new String[] { OpportunityStatus.New, APDocStatus.Open };
			object[] parms = new object[] { (object)statusList };

			PXSelectBase<CROpportunity> defaultCmdContactAddr = new PXSelectJoin<CROpportunity,
				InnerJoin<CRAddress, On<CROpportunity.opportunityAddressID, Equal<CRAddress.addressID>>>,
				Where<CRAddress.isValidated, Equal<False>,
				And<CROpportunity.status, In<Required<CROpportunity.status>>,
				And<Where<CRAddress.isDefaultAddress, Equal<False>,
				Or<CROpportunity.bAccountID, IsNull, And<CROpportunity.contactID, IsNull>>>>>>>(this);

			PXSelectBase<CROpportunity> defaultCmdShipAddr = new PXSelectJoin<CROpportunity,
				InnerJoin<CRShippingAddress, On<CROpportunity.shipAddressID, Equal<CRShippingAddress.addressID>>>,
				Where<CRShippingAddress.isValidated, Equal<False>,
				And<CRShippingAddress.isDefaultAddress, Equal<False>,
				And<CROpportunity.status, In<Required<CROpportunity.status>>>>>>(this);

			PXSelectBase<CROpportunity> defaultCmdBillAddr = new PXSelectJoin<CROpportunity,
				InnerJoin<CRBillingAddress, On<CROpportunity.billAddressID, Equal<CRBillingAddress.addressID>>>,
				Where<CRBillingAddress.isValidated, Equal<False>,
				And<CRBillingAddress.isDefaultAddress, Equal<False>,
				And<CROpportunity.status, In<Required<CROpportunity.status>>>>>>(this);

			if (!string.IsNullOrEmpty(filter?.Country))
			{
				parms = new object[] { (object)statusList, filter.Country };
				defaultCmdContactAddr.WhereAnd<Where<CRAddress.countryID, Equal<Required<CRAddress.countryID>>>>();
				defaultCmdShipAddr.WhereAnd<Where<CRShippingAddress.countryID, Equal<Required<CRShippingAddress.countryID>>>>();
				defaultCmdBillAddr.WhereAnd<Where<CRBillingAddress.countryID, Equal<Required<CRBillingAddress.countryID>>>>();
			}

			crContactAddresses = defaultCmdContactAddr.View.SelectMulti(parms);
			crShipAddresses = defaultCmdShipAddr.View.SelectMulti(parms);
			crBillAddresses = defaultCmdBillAddr.View.SelectMulti(parms);

			foreach (PXResult<CROpportunity, CRAddress> item in crContactAddresses)
			{
				var crContactAddr = (CRAddress)item;
				var crOppportunity = (CROpportunity)item;

				UnvalidatedAddress address = ConvertToUnvalidatedAddress(crContactAddr, crOppportunity,
					documentNbr: crOppportunity.OpportunityID,
					documentType: CR.MessagesNoPrefix.OpportunitiesDesc,
					status: new OpportunityStatus.ListAttribute().ValueLabelDic[crOppportunity.Status]);

				yield return DocumentAddresses.Insert(address);
			}

			foreach (PXResult<CROpportunity, CRShippingAddress> item in crShipAddresses)
			{
				var crShipAddr = (CRShippingAddress)item;
				var crOpportunity = (CROpportunity)item;

				UnvalidatedAddress address = ConvertToUnvalidatedAddress(crShipAddr, crOpportunity,
					documentNbr: crOpportunity.OpportunityID,
					documentType: CR.MessagesNoPrefix.OpportunitiesDesc,
					status: new OpportunityStatus.ListAttribute().ValueLabelDic[crOpportunity.Status]);

				yield return DocumentAddresses.Insert(address);
			}

			foreach (PXResult<CROpportunity, CRBillingAddress> item in crBillAddresses)
			{
				var crBillAddr = (CRBillingAddress)item;
				var crOpportunity = (CROpportunity)item;

				UnvalidatedAddress address = ConvertToUnvalidatedAddress(crBillAddr, crOpportunity,
					documentNbr: crOpportunity.OpportunityID,
					documentType: CR.MessagesNoPrefix.OpportunitiesDesc,
					status: new OpportunityStatus.ListAttribute().ValueLabelDic[crOpportunity.Status]);

				yield return DocumentAddresses.Insert(address);
			}
		}

		protected virtual IEnumerable AddPMQuoteAddresses(ValidateDocumentAddressFilter filter)
		{
			List<object> pmProjectQuoteAddresses = new List<object>();
			List<object> pmProjectQuoteShipAddresses = new List<object>();
			object[] parms = new object[] { };

			PXSelectBase<PMQuote> defaultCmdQuoteAddr = new PXSelectJoin<PMQuote,
				InnerJoin<CRAddress, On<PMQuote.opportunityAddressID, Equal<CRAddress.addressID>>>,
				Where<CRAddress.isValidated, Equal<False>,
				And<PMQuote.quoteType, Equal<CRQuoteTypeAttribute.project>,
				And<Where<CRAddress.isDefaultAddress, Equal<False>,
				Or<PMQuote.bAccountID, IsNull, And<PMQuote.contactID, IsNull>>>>>>>(this);

			PXSelectBase<PMQuote> defaultCmdShipAddr = new PXSelectJoin<PMQuote,
				InnerJoin<CRShippingAddress, On<PMQuote.shipAddressID, Equal<CRShippingAddress.addressID>>>,
				Where<CRShippingAddress.isDefaultAddress, Equal<False>,
				And<CRShippingAddress.isValidated, Equal<False>,
				And<PMQuote.quoteType, Equal<CRQuoteTypeAttribute.project>>>>>(this);

			if (!string.IsNullOrEmpty(filter?.Country))
			{
				parms = new object[] { filter.Country };
				defaultCmdQuoteAddr.WhereAnd<Where<CRAddress.countryID, Equal<Required<CRAddress.countryID>>>>();
				defaultCmdShipAddr.WhereAnd<Where<CRShippingAddress.countryID, Equal<Required<CRShippingAddress.countryID>>>>();
			}

			pmProjectQuoteAddresses = defaultCmdQuoteAddr.View.SelectMulti(parms);
			pmProjectQuoteShipAddresses = defaultCmdShipAddr.View.SelectMulti(parms);

			foreach (PXResult<PMQuote, CRAddress> item in pmProjectQuoteAddresses)
			{
				var pmQuoteAddress = (CRAddress)item;
				var pmQuote = (PMQuote)item;

				UnvalidatedAddress address = ConvertToUnvalidatedAddress(pmQuoteAddress, pmQuote,
					documentNbr: pmQuote.QuoteNbr,
					documentType: CR.MessagesNoPrefix.ProjectsQuoteDesc,
					status: new PMQuoteStatusAttribute().ValueLabelDic[pmQuote.Status]);

				yield return DocumentAddresses.Insert(address);
			}

			foreach (PXResult<PMQuote, CRShippingAddress> item in pmProjectQuoteShipAddresses)
			{
				var pmShipAddr = (CRShippingAddress)item;
				var pmQuote = (PMQuote)item;

				UnvalidatedAddress address = ConvertToUnvalidatedAddress(pmShipAddr, pmQuote,
					documentNbr: pmQuote.QuoteNbr,
					documentType: CR.MessagesNoPrefix.ProjectsQuoteDesc,
					status: new PMQuoteStatusAttribute().ValueLabelDic[pmQuote.Status]);

				yield return DocumentAddresses.Insert(address);
			}
		}
		#endregion
	}
}
