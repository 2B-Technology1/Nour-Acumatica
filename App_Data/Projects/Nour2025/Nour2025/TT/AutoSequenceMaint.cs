using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.AP;


namespace Maintenance
{
    public class AutoSequenceMaint : PXGraph<AutoSequenceMaint>
    {
        public PXCancel<AutoSequence> Cancel;
        public PXSave<AutoSequence> Save;

        public PXSelect<AutoSequence> sequences;

        #region Event Handlers
        protected void AutoSequence_Type_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {

            var row = (AutoSequence)e.Row;
            if (row.Type == ARPaymentType.Payment || row.Type == ARPaymentType.Prepayment || row.Type == ARPaymentType.Refund || row.Type == ARPaymentType.VoidPayment || row.Type == "Transaction")
                row.Vendor = false;

            if (row.Type == APPaymentType.Check || row.Type == APPaymentType.VoidCheck )
                row.Vendor = true;
        }
        #endregion
    }
}