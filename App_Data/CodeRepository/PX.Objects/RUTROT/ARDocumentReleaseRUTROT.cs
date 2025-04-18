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
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;


namespace PX.Objects.RUTROT
{
	[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2021R2)]
	public class ARDocumentReleaseRUTROT : PXGraphExtension<ARDocumentRelease>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.rutRotDeduction>();
		}

		public override void Initialize()
		{
			base.Initialize();

			Base.ARDocumentList.SetProcessDelegate(
				delegate (List<BalancedARDocument> list)
				{

						List<ARRegister> newlist = new List<ARRegister>(list.Count);
						foreach (BalancedARDocument doc in list)
						{
							newlist.Add(doc);
						}

					foreach (ARRegister doc in newlist)
					{
						using (PXTransactionScope ts = new PXTransactionScope())
						{
							ARInvoiceEntry graph = PXGraph.CreateInstance<ARInvoiceEntry>();

							if (RUTROTHelper.IsNeedBalancing(graph, RUTROTBalanceOn.Release))
							{
								BalanceProc(graph, doc);
							}

							ARDocumentRelease.ReleaseDoc(new List<ARRegister> { doc }, true);

							ts.Complete();
						}
					}
				}
			);
		}

		private static void BalanceProc(ARInvoiceEntry graph, ARRegister register)
		{
			ARInvoice invoice = PXSelect<ARInvoice, Where<ARInvoice.docType, Equal<Required<ARRegister.docType>>, And<ARInvoice.refNbr,
				Equal<Required<ARRegister.refNbr>>>>>.Select(graph, register.DocType, register.RefNbr);

			if (RUTROTHelper.IsNeedBalancing(graph, invoice, RUTROTBalanceOn.Release))
			{
				RUTROT rutrot = PXSelect<RUTROT, Where<RUTROT.refNbr, Equal<Required<ARInvoice.refNbr>>,
						And<RUTROT.docType, Equal<Required<ARInvoice.docType>>>>>.Select(graph, invoice.RefNbr, invoice.DocType);

				RUTROTHelper.BalanceARInvoiceRUTROT(graph, invoice, OnRelease: true, rutrot: rutrot);

				RUTROTHelper.CreateAdjustment(graph, invoice, rutrot);
			}
		}
	}
}