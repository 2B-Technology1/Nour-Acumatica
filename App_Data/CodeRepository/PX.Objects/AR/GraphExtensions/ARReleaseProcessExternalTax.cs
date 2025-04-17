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
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.SO;
using PX.Objects.TX;
using PX.TaxProvider;

namespace PX.Objects.AR
{
    public class ARReleaseProcessExternalTax : PXGraphExtension<ARReleaseProcess>
    {
	    public static bool IsActive()
	    {
		    return PXAccess.FeatureInstalled<FeaturesSet.avalaraTax>();
	    }

		protected Func<PXGraph, string, ITaxProvider> TaxProviderFactory;

		public ARReleaseProcessExternalTax()
	    {
		    TaxProviderFactory = ExternalTax.TaxProviderFactory;
	    }

	    public virtual bool IsExternalTax(string taxZoneID)
	    {
		    return ExternalTax.IsExternalTax(Base, taxZoneID);
	    }

	    protected Lazy<SOInvoiceEntry> LazySoInvoiceEntry =
		    new Lazy<SOInvoiceEntry>(() => PXGraph.CreateInstance<SOInvoiceEntry>());

		protected Lazy<ARInvoiceEntry> LazyArInvoiceEntry =
		    new Lazy<ARInvoiceEntry>(() => PXGraph.CreateInstance<ARInvoiceEntry>());

		[PXOverride]
        public virtual ARRegister OnBeforeRelease(ARRegister ardoc)
        {
            var invoice = ardoc as ARInvoice;

			if (invoice == null && ARInvoiceType.DrCr(ardoc.DocType) != null)
			{
				invoice = Base.ARInvoice_DocType_RefNbr.Select(ardoc.DocType, ardoc.RefNbr);
			}

            if (invoice == null || invoice.IsTaxValid == true || !IsExternalTax(invoice.TaxZoneID))
                return ardoc;

			ARInvoiceEntry graph = invoice.OrigModule == GL.BatchModule.SO
				? LazySoInvoiceEntry.Value
				: LazyArInvoiceEntry.Value;
			graph.Clear();

			return graph.RecalculateExternalTax(invoice);
		}

		[PXOverride]
		public virtual ARInvoice CommitExternalTax(ARInvoice doc)
		{
			if (doc != null && doc.IsTaxValid == true && doc.NonTaxable == false && IsExternalTax(doc.TaxZoneID) && doc.InstallmentNbr == null && doc.IsTaxPosted != true)
			{
				if (TaxPluginMaint.IsActive(Base, doc.TaxZoneID))
				{
					var service = ExternalTax.TaxProviderFactory(Base, doc.TaxZoneID);

					ARInvoiceEntry ie = PXGraph.CreateInstance<ARInvoiceEntry>();
					ie.Document.Current = doc;
					ARInvoiceEntryExternalTax ieExt = ie.GetExtension<ARInvoiceEntryExternalTax>();
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
							doc.WarningMessage = PXMessages.LocalizeFormatNoPrefixNLA(Messages.PostingToExternalTaxProviderFailed, sb.ToString())
								.AppendWithDot(PXMessages.LocalizeNoPrefix(Messages.ExternalTaxPostFailUsePostTaxForm));
						}
					}
				}
			}

			return doc;
		}
	}
}
