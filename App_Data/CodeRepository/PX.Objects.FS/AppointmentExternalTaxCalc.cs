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
    public class AppointmentExternalTaxCalc : PXGraph<AppointmentExternalTaxCalc>
    {
        [PXFilterable]
        public PXProcessingJoin<FSAppointment,
               InnerJoin<TaxZone,
               On<
                   TaxZone.taxZoneID, Equal<FSAppointment.taxZoneID>>>,
               Where<
                   TaxZone.isExternal, Equal<True>,
                   And<FSAppointment.isTaxValid, Equal<False>>>> Items;

        public AppointmentExternalTaxCalc()
        {
            Items.SetProcessDelegate(
                delegate (List<FSAppointment> list)
                {
                    List<FSAppointment> newlist = new List<FSAppointment>(list.Count);
                    foreach (FSAppointment doc in list)
                    {
                        newlist.Add(doc);
                    }
                    Process(newlist, true);
                }
            );

        }

        public static void Process(FSAppointment doc)
        {
            List<FSAppointment> list = new List<FSAppointment>();

            list.Add(doc);
            Process(list, false);
        }

        public static void Process(List<FSAppointment> list, bool isMassProcess)
        {
            AppointmentEntry appointmentEntryGraph = PXGraph.CreateInstance<AppointmentEntry>();
            
            for (int i = 0; i < list.Count; i++)
            {
                try
                {
                    appointmentEntryGraph.Clear();
                    appointmentEntryGraph.AppointmentRecords.Current = appointmentEntryGraph.AppointmentRecords.Search<FSAppointment.refNbr>(
                        list[i].RefNbr, list[i].SrvOrdType);
                    appointmentEntryGraph.CalculateExternalTax(appointmentEntryGraph.AppointmentRecords.Current);
                    PXProcessing<FSAppointment>.SetInfo(i, ActionsMessages.RecordProcessed);
                }
                catch (Exception e)
                {
                    if (isMassProcess)
                    {
                        PXProcessing<FSAppointment>.SetError(i, e is PXOuterException ? e.Message + "\r\n" + String.Join("\r\n", ((PXOuterException)e).InnerMessages) : e.Message);
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
