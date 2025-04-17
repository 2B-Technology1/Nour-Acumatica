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
using PX.Data;

namespace PX.Objects.AP
{
	public class APExternalTaxCalc : PXGraph<APExternalTaxCalc>
	{
		[PXFilterable]
		public PXProcessingJoin<APInvoice,
		InnerJoin<TX.TaxZone, On<TX.TaxZone.taxZoneID, Equal<APInvoice.taxZoneID>>>,
		Where<TX.TaxZone.isExternal, Equal<True>,
			And<APInvoice.isTaxValid, Equal<False>,
			And<APInvoice.released, Equal<False>>>>> Items;

		public APExternalTaxCalc()
		{
			Items.SetProcessDelegate(
				delegate(List<APInvoice> list)
				{
					List<APInvoice> newlist = new List<APInvoice>(list.Count);
					foreach (APInvoice doc in list)
					{
						newlist.Add(doc);
					}
					Process(newlist, true);
				});
		}

		public static List<APInvoice> Process(List<APInvoice> list, bool isMassProcess)
		{
			List<APInvoice> listWithTax = new List<APInvoice>(list.Count);
			APInvoiceEntry rg = PXGraph.CreateInstance<APInvoiceEntry>();
			for (int i = 0; i < list.Count; i++)
			{
				try
				{
					rg.Clear();
					rg.Document.Current = PXSelect<APInvoice, Where<APInvoice.docType, Equal<Required<APInvoice.docType>>, And<APInvoice.refNbr, Equal<Required<APInvoice.refNbr>>>>>.Select(rg, list[i].DocType, list[i].RefNbr);
					listWithTax.Add(rg.CalculateExternalTax(rg.Document.Current));
					PXProcessing<APInvoice>.SetInfo(i, ActionsMessages.RecordProcessed);
				}
				catch (Exception e)
				{
					if (isMassProcess)
					{
						PXProcessing<APInvoice>.SetError(i, e is PXOuterException ? e.Message + "\r\n" + String.Join("\r\n", ((PXOuterException)e).InnerMessages) : e.Message);
					}
					else
					{
						throw new PXMassProcessException(i, e);
					}
				}
			}

			return listWithTax;
		}
	}
}