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
using PX.Objects.Common;
using System.Diagnostics;

namespace PX.Objects.PM
{
	public class PMExternalTaxCalc : PXGraph<PMExternalTaxCalc>
	{
		[PXFilterable]
		public PXProcessingJoin<PMProforma,
		InnerJoin<TX.TaxZone, On<TX.TaxZone.taxZoneID, Equal<PMProforma.taxZoneID>>>,
		Where<TX.TaxZone.isExternal, Equal<True>,
			And<PMProforma.isTaxValid, Equal<False>>>> Items;

		public PMExternalTaxCalc()
		{
			Items.SetProcessDelegate(
				delegate(List<PMProforma> list)
				{
					List<PMProforma> newlist = new List<PMProforma>(list.Count);
					foreach (PMProforma doc in list)
					{
						newlist.Add(doc);
					}
					Process(newlist, true);
				}
			);

		}

		public static void Process(PMProforma doc)
		{
			List<PMProforma> list = new List<PMProforma>();

			list.Add(doc);
			Process(list, false);
		}
		
		public static void Process(List<PMProforma> list, bool isMassProcess)
		{
			Stopwatch sw = new Stopwatch();
			ProformaEntry rg = PXGraph.CreateInstance<ProformaEntry>();
			rg.SuppressRowSeleted = true;
			for (int i = 0; i < list.Count; i++)
			{
				try
				{
					rg.Document.Current = PXSelect<PMProforma, Where<PMProforma.refNbr, Equal<Required<PMProforma.refNbr>>, And<PMProforma.revisionID, Equal<Required<PMProforma.revisionID>>>>>.Select(rg, list[i].RefNbr, list[i].RevisionID);
					rg.CalculateExternalTax(rg.Document.Current);
					PXProcessing<PMProforma>.SetInfo(i, ActionsMessages.RecordProcessed);
				}
				catch (Exception e)
				{
					if (isMassProcess)
					{
						PXProcessing<PMProforma>.SetError(i, e is PXOuterException ? e.Message + Environment.NewLine + string.Join(Environment.NewLine, ((PXOuterException)e).InnerMessages) : e.Message);
					}
					else
					{
						throw new PXMassProcessException(i, e);
					}
				}

			}
		}

	}
}
