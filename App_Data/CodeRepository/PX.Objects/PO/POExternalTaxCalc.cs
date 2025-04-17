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
using PX.Data;
using PX.Objects.AR;

namespace PX.Objects.PO
{
	public class POExternalTaxCalc : PXGraph<POExternalTaxCalc>
	{
		[PXFilterable]
		public PXProcessingJoin<POOrder,
		InnerJoin<TX.TaxZone, On<TX.TaxZone.taxZoneID, Equal<POOrder.taxZoneID>>>,
		Where<TX.TaxZone.isExternal, Equal<True>,
			And<POOrder.isTaxValid, Equal<False>>>> Items;

		public POExternalTaxCalc()
		{
			Items.SetProcessDelegate(
				delegate(List<POOrder> list)
				{
					List<POOrder> newlist = new List<POOrder>(list.Count);
					foreach (POOrder doc in list)
					{
						newlist.Add(doc);
					}
					Process(newlist, true);
				}
			);

		}

		public static void Process(POOrder doc)
		{
			List<POOrder> list = new List<POOrder>();

			list.Add(doc);
			Process(list, false);
		}

//		public static void Process(POReceipt doc)
//		{
//			List<POReceipt> list = new List<POReceipt>();
//
//			list.Add(doc);
//			Process(list, false);
//		}

		public static void Process(List<POOrder> list, bool isMassProcess)
		{
			POOrderEntry rg = PXGraph.CreateInstance<POOrderEntry>();
			for (int i = 0; i < list.Count; i++)
			{
				try
				{
					rg.Clear();
					rg.Document.Current = PXSelect<POOrder, Where<POOrder.orderType, Equal<Required<POOrder.orderType>>, And<POOrder.orderNbr, Equal<Required<POOrder.orderNbr>>>>>.Select(rg, list[i].OrderType, list[i].OrderNbr);
					rg.CalculateExternalTax(rg.Document.Current);
					PXProcessing<POOrder>.SetInfo(i, ActionsMessages.RecordProcessed);
				}
				catch (Exception e)
				{
					if (isMassProcess)
					{
						PXProcessing<POOrder>.SetError(i, e is PXOuterException ? e.Message + "\r\n" + String.Join("\r\n", ((PXOuterException)e).InnerMessages) : e.Message);
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
