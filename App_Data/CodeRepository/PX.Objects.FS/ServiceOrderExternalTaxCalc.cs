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
using PX.Objects.TX;
using System;
using System.Collections.Generic;

namespace PX.Objects.FS
{
    public class ServiceOrderExternalTaxCalc : PXGraph<ServiceOrderExternalTaxCalc>
    {
        [PXFilterable]
        public PXProcessingJoin<FSServiceOrder,
               InnerJoin<TaxZone,
               On<
                   TaxZone.taxZoneID, Equal<FSServiceOrder.taxZoneID>>>,
               Where<
                   TaxZone.isExternal, Equal<True>,
                   And<FSServiceOrder.isTaxValid, Equal<False>>>> Items;

        public ServiceOrderExternalTaxCalc()
        {
            Items.SetProcessDelegate(
                delegate (List<FSServiceOrder> list)
                {
                    List<FSServiceOrder> newlist = new List<FSServiceOrder>(list.Count);
                    foreach (FSServiceOrder doc in list)
                    {
                        newlist.Add(doc);
                    }
                    Process(newlist, true);
                }
            );

        }

        public static void Process(FSServiceOrder doc)
        {
            List<FSServiceOrder> list = new List<FSServiceOrder>();

            list.Add(doc);
            Process(list, false);
        }

        public static void Process(List<FSServiceOrder> list, bool isMassProcess)
        {
            ServiceOrderEntry serviceOrderEntryGraph = PXGraph.CreateInstance<ServiceOrderEntry>();

            for (int i = 0; i < list.Count; i++)
            {
                try
                {
                    serviceOrderEntryGraph.Clear();
					serviceOrderEntryGraph.ServiceOrderRecords.Current = list[i];
					serviceOrderEntryGraph.CalculateExternalTax(list[i]);
                    PXProcessing<FSServiceOrder>.SetInfo(i, ActionsMessages.RecordProcessed);
                }
                catch (Exception e)
                {
                    if (isMassProcess)
                    {
                        PXProcessing<FSServiceOrder>.SetError(i, e is PXOuterException ? e.Message + "\r\n" + String.Join("\r\n", ((PXOuterException)e).InnerMessages) : e.Message);
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
