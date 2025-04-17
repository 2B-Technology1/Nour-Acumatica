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
using PX.Objects.Common.Exceptions;
using PX.Objects.CS;
using System.Linq;

namespace PX.Objects.AR
{
	public class ARReleaseProcessVATRecognitionOnPrepayments : PXGraphExtension<ARReleaseProcess>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.vATRecognitionOnPrepayments>();
		}

		public delegate void PerformBasicReleaseChecksDelegate(PXGraph selectGraph, ARRegister document);
		[PXOverride]
		public void PerformBasicReleaseChecks(PXGraph selectGraph, ARRegister document, PerformBasicReleaseChecksDelegate baseMethod)
		{
			baseMethod(selectGraph, document);

			ARInvoice documentAsInvoice = PXSelect<ARInvoice,
				Where<ARInvoice.docType, Equal<Required<ARInvoice.docType>>,
					And<ARInvoice.refNbr, Equal<Required<ARInvoice.refNbr>>>>>
				.Select(Base, document.DocType, document.RefNbr);

			if (documentAsInvoice?.DocType == ARInvoiceType.PrepaymentInvoice)
			{
				bool isMultipleInstallment = PXSelect<Terms,
					Where<Terms.termsID, Equal<Required<ARInvoice.termsID>>,
						And<Terms.installmentType, Equal<TermsInstallmentType.multiple>>>>
					.SelectSingleBound(selectGraph, null, new object[] { documentAsInvoice.TermsID })
					.Any();

				if (isMultipleInstallment)
				{
					throw new ReleaseException(Messages.CannotReleasePrepaymentInvoiceWithMultipleInstallmentCreditTerms);
				}

				if (documentAsInvoice.PrepaymentAccountID == null)
				{
					throw new ReleaseException(AR.Messages.CannotReleasePrepaymentInvoiceWithEmptyPrepaymentAccount);
				}

				if (PXAccess.FeatureInstalled<FeaturesSet.subAccount>() && documentAsInvoice.PrepaymentSubID == null)
				{
					throw new ReleaseException(AR.Messages.CannotReleasePrepaymentInvoiceWithEmptyPrepaymentSubaccount);
				}
			}
		}
	}
}
