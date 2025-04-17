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
using PX.Objects.CR.Standalone;
using PX.Objects.CT;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PX.Objects.PM
{
	public class ValidatePMDocumentAddressProcess : ValidateDocumentAddressGraph<ValidatePMDocumentAddressProcess>
	{
		#region Event Handlers
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[DocumentTypeField.List(DocumentTypeField.SetOfValues.PMRelatedDocumentTypes)]
		public virtual void _(Events.CacheAttached<ValidateDocumentAddressFilter.documentType> e) { }
		#endregion

		#region Method Overrides
		protected override IEnumerable GetAddressRecords(ValidateDocumentAddressFilter filter)
		{
			IEnumerable result = new List<UnvalidatedAddress>();
			string docTypeFilter = filter.DocumentType.Trim();

			if (docTypeFilter.Equals(DocumentTypeField.Projects))
			{
				result = AddPMProjectAddresses(filter);
			}
			else if (docTypeFilter.Equals(DocumentTypeField.ProjectsQuote))
			{
				result = AddPMQuoteAddresses(filter);
			}

			return result;
		}
		#endregion

		#region Protected Methods
		protected virtual IEnumerable AddPMProjectAddresses(ValidateDocumentAddressFilter filter)
		{
			List<object> billAddresses = new List<object>();
			object[] statusList = new String[] { ProjectStatus.Planned, ProjectStatus.Active };
			object[] parms = new object[] { (object)statusList };

			PXSelectBase<PMProject> billAddrCmd = new PXSelectJoin<PMProject,
				InnerJoin<PMAddress, On<PMProject.billAddressID, Equal<PMAddress.addressID>>>,
				Where<PMAddress.isDefaultBillAddress, Equal<False>,
				And<PMAddress.isValidated, Equal<False>,
				And<PMProject.status, In<Required<PMProject.status>>,
				And<PMProject.baseType, Equal<CTPRType.project>,
				And<PMProject.nonProject, Equal<False>>>>>>>(this);

			if (!string.IsNullOrEmpty(filter?.Country))
			{
				parms = new object[] { (object)statusList, filter.Country };
				billAddrCmd.WhereAnd<Where<PMAddress.countryID, Equal<Required<PMAddress.countryID>>>>();
			}

			billAddresses = billAddrCmd.View.SelectMulti(parms);

			foreach (PXResult<PMProject, PMAddress> item in billAddresses)
			{
				var pmProjectBillAddr = (PMAddress)item;
				var pmProject = (PMProject)item;

				UnvalidatedAddress address = ConvertToUnvalidatedAddress(pmProjectBillAddr, pmProject,
					documentNbr: pmProject.ContractCD,
					documentType: CR.MessagesNoPrefix.ProjectsDesc,
					status: new ProjectStatus.ListAttribute().ValueLabelDic[pmProject.Status]);

				yield return DocumentAddresses.Insert(address);
			}
		}

		protected virtual IEnumerable AddPMQuoteAddresses(ValidateDocumentAddressFilter filter)
		{
			List<object> quoteAddresses = new List<object>();
			List<object> shipAddresses = new List<object>();
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

			quoteAddresses = defaultCmdQuoteAddr.View.SelectMulti(parms);
			shipAddresses = defaultCmdShipAddr.View.SelectMulti(parms);

			foreach (PXResult<PMQuote, CRAddress> item in quoteAddresses)
			{
				var pmQuoteAddress = (CRAddress)item;
				var pmQuote = (PMQuote)item;

				UnvalidatedAddress address = ConvertToUnvalidatedAddress(pmQuoteAddress, pmQuote,
					documentNbr: pmQuote.QuoteNbr,
					documentType: CR.MessagesNoPrefix.ProjectsQuoteDesc,
					status: new PMQuoteStatusAttribute().ValueLabelDic[pmQuote.Status]);

				yield return DocumentAddresses.Insert(address);
			}

			foreach (PXResult<PMQuote, CRShippingAddress> item in shipAddresses)
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
