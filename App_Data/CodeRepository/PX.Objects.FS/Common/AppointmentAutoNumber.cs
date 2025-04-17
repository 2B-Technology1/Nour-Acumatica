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
using System;
using System.Linq;

namespace PX.Objects.FS
{
    public class AppointmentAutoNumberAttribute : AlternateAutoNumberAttribute, IPXRowInsertingSubscriber
    {
        protected override string GetInitialRefNbr(string baseRefNbr)
        {
            return baseRefNbr.Trim() + "-1";
        }

        public AppointmentAutoNumberAttribute(Type setupField, Type dateField)
            : base(setupField, dateField)
        {
        }

        void IPXRowInsertingSubscriber.RowInserting(PXCache sender, PXRowInsertingEventArgs e)
        {
        }

        protected override string GetNewNumberSymbol(string numberingID)
        {
            return " <NEW>";
        }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);
            base.UserNumbering = false;
        }

        /// <summary>
        /// Allows to calculate the <c>RefNbr</c> sequence when trying to insert a new register.
        /// </summary>
        protected override bool SetRefNbr(PXCache cache, object row)
        {
            FSAppointment fsAppointmentRow = (FSAppointment)row;

            if (fsAppointmentRow.SOID == null || fsAppointmentRow.SOID < 0)
            {
                return false;
            }

            FSAppointment fsAppointmentRowTmp = PXSelectReadonly<FSAppointment,
                                                Where<
                                                    FSAppointment.sOID, Equal<Current<FSAppointment.sOID>>>,
                                                OrderBy<
                                                    Desc<FSAppointment.appointmentID>>>
                                                .SelectWindowed(cache.Graph, 0, 1);

            string lastRefNbr = fsAppointmentRowTmp?.RefNbr;

            fsAppointmentRow.RefNbr = GetNextRefNbr(fsAppointmentRow.SORefNbr, lastRefNbr);

            return true;
        }
    }
}