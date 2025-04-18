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
using System.Diagnostics;

namespace PX.Objects.SO
{
	public class SOExternalTaxCalc : PXGraph<SOExternalTaxCalc>
	{
		[PXFilterable]
		public PXProcessingJoin<SOOrder,
		InnerJoin<TX.TaxZone, On<TX.TaxZone.taxZoneID, Equal<SOOrder.taxZoneID>>>,
		Where<TX.TaxZone.isExternal, Equal<True>,
			And<SOOrder.isTaxValid, Equal<False>>>> Items;

		public SOExternalTaxCalc()
		{
			Items.SetProcessDelegate(
				delegate(List<SOOrder> list)
				{
					List<SOOrder> newlist = new List<SOOrder>(list.Count);
					foreach (SOOrder doc in list)
					{
						newlist.Add(doc);
					}
					Process(newlist, true);
				}
			);

		}

		public static void Process(SOOrder doc)
		{
			List<SOOrder> list = new List<SOOrder>();

			list.Add(doc);
			Process(list, false);
		}
		
		public static void Process(List<SOOrder> list, bool isMassProcess)
		{
			Stopwatch sw = new Stopwatch();
			sw.Start();
			SOOrderEntry rg = PXGraph.CreateInstance<SOOrderEntry>();
			sw.Stop();
			Debug.Print("{0} PXGraph.CreateInstance<SOOrderEntry> in {1} millisec", DateTime.Now.TimeOfDay, sw.ElapsedMilliseconds); 
			for (int i = 0; i < list.Count; i++)
			{
				try
				{
					rg.Clear();
					sw.Reset();
					sw.Start();
					rg.Document.Current = PXSelect<SOOrder, Where<SOOrder.orderType, Equal<Required<SOOrder.orderType>>, And<SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>>>>.Select(rg, list[i].OrderType, list[i].OrderNbr);
					sw.Stop();
					Debug.Print("{0} Select SOOrder in {1} millisec", DateTime.Now.TimeOfDay, sw.ElapsedMilliseconds); 
					rg.CalculateExternalTax(rg.Document.Current);
					PXProcessing<SOOrder>.SetInfo(i, ActionsMessages.RecordProcessed);
				}
				catch (Exception e)
				{
					if (isMassProcess)
					{
						PXProcessing<SOOrder>.SetError(i, e is PXOuterException ? e.Message + "\r\n" + String.Join("\r\n", ((PXOuterException)e).InnerMessages) : e.Message);
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
