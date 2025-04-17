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
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.PM;
using System;
using System.Collections;

namespace PX.Objects.CN.ProjectAccounting.AR.GraphExtensions
{
	public class ARPaymentEntryExt : PXGraphExtension<ARPaymentEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.construction>();
		}

		protected virtual void ARAdjust_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			ARAdjust adjustment = (ARAdjust)e.Row;

			PMProforma proforma = PXSelect<PMProforma, Where<PMProforma.aRInvoiceDocType, Equal<Required<ARAdjust.adjdDocType>>,
				And<PMProforma.aRInvoiceRefNbr, Equal<Required<ARAdjust.adjdRefNbr>>>>>.Select(Base, adjustment.AdjdDocType, adjustment.AdjdRefNbr);

			if (proforma != null && proforma.Corrected == true && proforma.Status != ProformaStatus.Closed)
			{
				if (Base.Document.Current.DocType == ARDocType.Payment)
				{
					sender.RaiseExceptionHandling<ARAdjust.adjdRefNbr>(adjustment, adjustment.AdjdRefNbr, new PXSetPropertyException(PX.Objects.PM.Messages.CannotPreparePayment, adjustment.AdjdRefNbr, proforma.RefNbr));
				}
				if (Base.Document.Current.DocType == ARDocType.CreditMemo)
				{
					sender.RaiseExceptionHandling<ARAdjust.adjdRefNbr>(adjustment, adjustment.AdjdRefNbr, new PXSetPropertyException(PX.Objects.PM.Messages.CannotReverseInvoice, adjustment.AdjdRefNbr, proforma.RefNbr));
				}
			}
		}

		[PXOverride]
		public virtual IEnumerable Release(PXAdapter adapter, Func<PXAdapter, IEnumerable> baseHandler)
		{
			var document = Base.Document.Current;

			if (document?.DocType != ARPaymentType.CreditMemo)
			{
				return baseHandler(adapter);
			}

			foreach (var adjustment in Base.Adjustments.Select())
			{
				ARInvoice adjustedInvoice = PXResult.Unwrap<ARInvoice>(adjustment);

				if (adjustedInvoice != null)
				{
					PMProforma proforma = GetPMProformaOfARInvoice(adjustedInvoice);

					if (!AskUserApprovalIfProformaExistAndClosed(document.RefNbr, proforma, adjustedInvoice))
					{
						return adapter.Get();
					}
				}
			}

			return baseHandler(adapter);
		}

		protected virtual PMProforma GetPMProformaOfARInvoice(ARInvoice invoice)
			=> PXSelect<PMProforma, Where<PMProforma.aRInvoiceDocType, Equal<Required<ARInvoice.docType>>,
									And<PMProforma.aRInvoiceRefNbr, Equal<Required<ARInvoice.refNbr>>>>>
			.Select(Base, invoice.DocType, invoice.RefNbr);

		protected virtual bool AskUserApprovalIfProformaExistAndClosed(string creditMemoRefNbr, PMProforma proforma, ARInvoice invoice)
		{
			if (proforma?.Status != ProformaStatus.Closed || invoice.Released != true)
			{
				return true;
			}

			string localizedHeader = PX.Objects.PM.Messages.InvoiceWithProformaReverseDialogHeader;

			string localizedMessage = PXMessages.LocalizeFormatNoPrefix(
				PX.Objects.PM.Messages.InvoiceWithProformaCreditMemoApplyWarning, proforma.RefNbr, creditMemoRefNbr, invoice.RefNbr);

			return Base.Document.View.Ask(null, localizedHeader, localizedMessage, MessageButtons.YesNo, MessageIcon.Warning) == WebDialogResult.Yes;
		}
	}
}
