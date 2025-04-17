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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.TX;
using PX.TaxProvider;

namespace PX.Objects.AP
{
	public class APReleaseProcessExternalTax : PXGraphExtension<APReleaseProcess>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.avalaraTax>();
		}

		protected Func<PXGraph, string, ITaxProvider> TaxProviderFactory;
		
		public APReleaseProcessExternalTax()
		{
			TaxProviderFactory = ExternalTax.TaxProviderFactory;
		}

		public bool IsExternalTax(string taxZoneID)
		{
			return ExternalTax.IsExternalTax(Base, taxZoneID);
		}

		protected Lazy<APInvoiceEntry> LazyApInvoiceEntry =
			new Lazy<APInvoiceEntry>(() => PXGraph.CreateInstance<APInvoiceEntry>());

		[PXOverride]
		public virtual APRegister OnBeforeRelease(APRegister apdoc)
		{
			var invoice = apdoc as APInvoice;

			if (invoice == null || invoice.IsTaxValid == true || !IsExternalTax(invoice.TaxZoneID))
				return apdoc;

			var rg = LazyApInvoiceEntry.Value;
			rg.Clear();

			rg.Document.Current = PXSelect<APInvoice, Where<APInvoice.docType, Equal<Required<APInvoice.docType>>, And<APInvoice.refNbr, Equal<Required<APInvoice.refNbr>>>>>.Select(rg, invoice.DocType, invoice.RefNbr);

			return rg.CalculateExternalTax(rg.Document.Current);
		}

		[PXOverride]
		public virtual APInvoice CommitExternalTax(APInvoice doc)
		{
			if (doc != null && doc.IsTaxValid == true && doc.NonTaxable == false && IsExternalTax(doc.TaxZoneID) && doc.InstallmentNbr == null && doc.IsTaxPosted != true)
			{
				if (TaxPluginMaint.IsActive(Base, doc.TaxZoneID))
				{
					var service = TaxProviderFactory(Base, doc.TaxZoneID);

					APInvoiceEntry ie = PXGraph.CreateInstance<APInvoiceEntry>();
					ie.Document.Current = doc;
					APInvoiceEntryExternalTax ieExt = ie.GetExtension<APInvoiceEntryExternalTax>();
					CommitTaxRequest request = ieExt.BuildCommitTaxRequest(doc);

					CommitTaxResult result = service.CommitTax(request);
					if (result.IsSuccess)
					{
						doc.IsTaxPosted = true;
					}
					else
					{
						//show as warning.
						StringBuilder sb = new StringBuilder();
						foreach (var msg in result.Messages)
						{
							sb.AppendLine(msg);
						}

						if (sb.Length > 0)
						{
							doc.WarningMessage = PXMessages.LocalizeFormatNoPrefixNLA(AR.Messages.PostingToExternalTaxProviderFailed, sb.ToString())
								.AppendWithDot(PXMessages.LocalizeNoPrefix(AR.Messages.ExternalTaxPostFailUsePostTaxForm));
						}
					}
				}
			}

			return doc;
		}

		public virtual TaxDocumentType GetTaxDocumentType(APInvoice invoice)
		{
			switch (invoice.DrCr)
			{
				case DrCr.Debit:
					return TaxDocumentType.PurchaseInvoice;
				case DrCr.Credit:
					return TaxDocumentType.ReturnInvoice;

				default:
					throw new PXException(Messages.DocTypeNotSupported);
			}
		}
	}
}
